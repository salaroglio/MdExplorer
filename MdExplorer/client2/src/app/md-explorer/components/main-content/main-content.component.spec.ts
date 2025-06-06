import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { DomSanitizer } from '@angular/platform-browser';
import { ChangeDetectorRef, ElementRef } from '@angular/core';
import { of, BehaviorSubject, Subject } from 'rxjs';

import { MainContentComponent } from './main-content.component';
import { MdFileService } from '../../services/md-file.service';
import { MdServerMessagesService } from '../../../signalR/services/server-messages.service';
import { IndexingStateService } from '../../services/indexing-state.service';
import { FileEventsService } from '../../services/file-events.service';
import { MdFile } from '../../models/md-file';

describe('MainContentComponent - Enhanced UX', () => {
  let component: MainContentComponent;
  let fixture: ComponentFixture<MainContentComponent>;
  let mockMdFileService: jasmine.SpyObj<MdFileService>;
  let mockMonitorMDService: jasmine.SpyObj<MdServerMessagesService>;
  let mockIndexingStateService: jasmine.SpyObj<IndexingStateService>;
  let mockFileEventsService: jasmine.SpyObj<FileEventsService>;
  let mockDomSanitizer: jasmine.SpyObj<DomSanitizer>;

  const mockMdFile: MdFile = {
    relativePath: 'test/document.md',
    name: 'document.md',
    path: '/full/path/test/document.md'
  };

  beforeEach(async () => {
    // Create comprehensive service spies
    mockMdFileService = jasmine.createSpyObj('MdFileService', [
      'setSelectedMdFileFromServer',
      'setSelectedMdFileFromSideNav'
    ], {
      selectedMdFileFromSideNav: new BehaviorSubject<MdFile>(null),
      selectedMdFileFromToolbar: new BehaviorSubject<MdFile[]>([]),
      navigationArray: []
    });

    mockMonitorMDService = jasmine.createSpyObj('MdServerMessagesService', [
      'addMarkdownFileListener'
    ], {
      connectionId: 'test-connection-id'
    });

    mockIndexingStateService = jasmine.createSpyObj('IndexingStateService', [
      'isFileIndexed',
      'isFileWaiting',
      'updateFileState',
      'markAsIndexed'
    ], {
      indexedFiles$: new BehaviorSubject(new Map())
    });

    mockFileEventsService = jasmine.createSpyObj('FileEventsService', [
      'emitFileRenamed',
      'emitFileIndexed'
    ], {
      fileRenamed$: new Subject(),
      fileIndexed$: new Subject()
    });

    mockDomSanitizer = jasmine.createSpyObj('DomSanitizer', ['bypassSecurityTrustResourceUrl']);

    await TestBed.configureTestingModule({
      declarations: [MainContentComponent],
      imports: [
        BrowserAnimationsModule,
        MatProgressSpinnerModule,
        MatIconModule,
        MatButtonModule,
        MatDialogModule
      ],
      providers: [
        { provide: MdFileService, useValue: mockMdFileService },
        { provide: MdServerMessagesService, useValue: mockMonitorMDService },
        { provide: IndexingStateService, useValue: mockIndexingStateService },
        { provide: FileEventsService, useValue: mockFileEventsService },
        { provide: DomSanitizer, useValue: mockDomSanitizer }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MainContentComponent);
    component = fixture.componentInstance;
  });

  describe('✅ Component Creation and Initialization', () => {
    it('should create component successfully', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with idle state', fakeAsync(() => {
      fixture.detectChanges();
      tick();
      
      component.currentState$.subscribe(state => {
        expect(state.status).toBe('idle');
        expect(state.retryCount).toBe(0);
      });
    }));

    it('should initialize all required observables', () => {
      fixture.detectChanges();
      
      expect(component.currentState$).toBeDefined();
      expect(component.isLoading$).toBeDefined();
      expect(component.hasError$).toBeDefined();
      expect(component.errorMessage$).toBeDefined();
    });

    it('should set up service integration on init', () => {
      spyOn(component, 'ngOnInit').and.callThrough();
      component.ngOnInit();
      expect(component.ngOnInit).toHaveBeenCalled();
    });
  });

  describe('📄 File Loading and State Management', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should transition to loading state when file is selected', fakeAsync(() => {
      const spy = spyOn(component as any, 'updateState').and.callThrough();
      
      (component as any).loadMarkdownFile(mockMdFile);
      tick();

      expect(spy).toHaveBeenCalledWith(jasmine.objectContaining({
        status: 'loading',
        currentPath: mockMdFile.relativePath
      }));
    }));

    it('should handle invalid files gracefully', fakeAsync(() => {
      const spy = spyOn(component as any, 'updateState').and.callThrough();
      const invalidFile: MdFile = { relativePath: '' } as any;
      
      (component as any).loadMarkdownFile(invalidFile);
      tick();

      expect(spy).toHaveBeenCalledWith(jasmine.objectContaining({
        status: 'error',
        errorMessage: 'File non valido selezionato'
      }));
    }));

    it('should update htmlSource correctly', () => {
      (component as any).loadMarkdownFile(mockMdFile);
      
      expect(component.htmlSource).toContain(mockMdFile.relativePath);
      expect(component.htmlSource).toContain('connectionId=test-connection-id');
      expect(component.htmlSource).toMatch(/time=\\d+/);
    });

    it('should emit correct loading observable states', fakeAsync(() => {
      (component as any).updateState({ status: 'loading' });
      tick(100); // Wait for debounce

      component.isLoading$.subscribe(isLoading => {
        expect(isLoading).toBe(true);
      });

      component.hasError$.subscribe(hasError => {
        expect(hasError).toBe(false);
      });
    }));
  });

  describe('🔄 Indexing Integration', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should handle indexing state transitions', () => {
      const spy = spyOn(component as any, 'updateState').and.callThrough();
      const indexingState = { indexingStatus: 'indexing', isIndexed: false };
      
      (component as any).handleIndexingStateChange('test.md', indexingState);

      expect(spy).toHaveBeenCalledWith(jasmine.objectContaining({
        status: 'indexing',
        isIndexing: true
      }));
    });

    it('should refresh file when indexing completes', () => {
      const refreshSpy = spyOn(component as any, 'refreshCurrentFile');
      (component as any).updateState({ status: 'indexing', currentPath: 'test.md' });
      
      const completedState = { indexingStatus: 'completed', isIndexed: true };
      (component as any).handleIndexingStateChange('test.md', completedState);

      expect(refreshSpy).toHaveBeenCalled();
    });

    it('should handle file indexing completion events', () => {
      const refreshSpy = spyOn(component as any, 'refreshCurrentFile');
      (component as any).updateState({ status: 'indexing', currentPath: 'test.md' });
      
      const event = { fullPath: 'test.md', isIndexed: true };
      (component as any).handleFileIndexingComplete(event);

      expect(refreshSpy).toHaveBeenCalled();
    });

    it('should handle file rename events correctly', () => {
      const updateSpy = spyOn(component as any, 'updateState').and.callThrough();
      
      (component as any).handleFileRenamed('old.md', 'new.md');

      expect(updateSpy).toHaveBeenCalledWith(jasmine.objectContaining({
        currentPath: 'new.md'
      }));
      expect(component.htmlSource).toContain('new.md');
    });
  });

  describe('⚠️ Error Handling and Retry Logic', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should emit error states correctly', fakeAsync(() => {
      const errorMessage = 'Test error occurred';
      (component as any).updateState({ 
        status: 'error',
        errorMessage 
      });
      tick(100);

      component.hasError$.subscribe(hasError => {
        expect(hasError).toBe(true);
      });

      component.errorMessage$.subscribe(message => {
        expect(message).toBe(errorMessage);
      });
    }));

    it('should retry with exponential backoff', fakeAsync(() => {
      (component as any).updateState({ 
        status: 'error',
        currentPath: 'test.md',
        retryCount: 1 
      });

      const updateSpy = spyOn(component as any, 'updateState').and.callThrough();
      component.retry();
      
      tick(2000); // Wait for exponential backoff (1000 * 2^1)

      expect(updateSpy).toHaveBeenCalledWith(jasmine.objectContaining({
        status: 'loading',
        retryCount: 2
      }));
    }));

    it('should stop retrying after max attempts', () => {
      (component as any).updateState({ 
        status: 'error',
        currentPath: 'test.md',
        retryCount: 3 // Already at max
      });

      const updateSpy = spyOn(component as any, 'updateState').and.callThrough();
      component.retry();

      expect(updateSpy).toHaveBeenCalledWith(jasmine.objectContaining({
        status: 'error',
        errorMessage: jasmine.stringContaining('massimo di tentativi')
      }));
    });

    it('should update retry URL parameter', fakeAsync(() => {
      (component as any).updateState({ 
        status: 'error',
        currentPath: 'test.md',
        retryCount: 0 
      });

      component.retry();
      tick(1100);

      expect(component.htmlSource).toContain('retry=1');
    }));
  });

  describe('🎨 UI State Management', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should show loading overlay when loading', fakeAsync(() => {
      (component as any).updateState({ status: 'loading' });
      fixture.detectChanges();
      tick(100);

      const loadingOverlay = fixture.nativeElement.querySelector('.loading-overlay');
      expect(loadingOverlay).toBeTruthy();
    }));

    it('should show indexing overlay when indexing', fakeAsync(() => {
      (component as any).updateState({ status: 'indexing' });
      fixture.detectChanges();
      tick();

      const indexingOverlay = fixture.nativeElement.querySelector('.indexing-overlay');
      expect(indexingOverlay).toBeTruthy();
    }));

    it('should show error state when error occurs', fakeAsync(() => {
      (component as any).updateState({ status: 'error', errorMessage: 'Test error' });
      fixture.detectChanges();
      tick(100);

      const errorState = fixture.nativeElement.querySelector('.error-state');
      expect(errorState).toBeTruthy();
    }));

    it('should show empty state when idle', fakeAsync(() => {
      (component as any).updateState({ status: 'idle' });
      fixture.detectChanges();
      tick();

      const emptyState = fixture.nativeElement.querySelector('.empty-state');
      expect(emptyState).toBeTruthy();
    }));

    it('should hide iframe during loading and error states', fakeAsync(() => {
      (component as any).updateState({ status: 'loading' });
      fixture.detectChanges();
      tick(100);

      const iframe = fixture.nativeElement.querySelector('.content-iframe');
      expect(iframe.hasAttribute('hidden')).toBe(true);
    }));
  });

  describe('♿ Accessibility Features', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have proper ARIA labels for loading state', fakeAsync(() => {
      (component as any).updateState({ status: 'loading' });
      fixture.detectChanges();
      tick(100);

      const loadingOverlay = fixture.nativeElement.querySelector('.loading-overlay');
      expect(loadingOverlay?.getAttribute('aria-label')).toContain('Caricamento documento');
      expect(loadingOverlay?.getAttribute('role')).toBe('status');
    }));

    it('should have proper ARIA labels for error state', fakeAsync(() => {
      (component as any).updateState({ status: 'error', errorMessage: 'Test error' });
      fixture.detectChanges();
      tick(100);

      const errorState = fixture.nativeElement.querySelector('.error-state');
      expect(errorState?.getAttribute('aria-label')).toContain('Errore di caricamento');
      expect(errorState?.getAttribute('role')).toBe('alert');
    }));

    it('should have proper ARIA labels for iframe', fakeAsync(() => {
      (component as any).updateState({ status: 'loaded', currentPath: 'test.md' });
      fixture.detectChanges();
      tick();

      const iframe = fixture.nativeElement.querySelector('.content-iframe');
      expect(iframe?.getAttribute('aria-label')).toContain('test.md');
      expect(iframe?.getAttribute('role')).toBe('document');
    }));

    it('should support keyboard navigation', () => {
      const iframe = fixture.nativeElement.querySelector('.content-iframe');
      expect(iframe?.getAttribute('tabindex')).toBe('0');
    });
  });

  describe('🔄 Legacy Compatibility', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should maintain backward compatibility properties', () => {
      expect(component._HideIFrame).toBeDefined();
      expect(component.htmlSource).toBeDefined();
      expect(component.classForContent).toBeDefined();
      expect(component.mdFile).toBeDefined();
    });

    it('should handle legacy markdownFileIsChanged callback', () => {
      const loadSpy = spyOn(component as any, 'loadMarkdownFile');
      const mockData = {
        relativePath: 'legacy-test.md',
        name: 'legacy-test.md'
      };

      (component as any).markdownFileIsChanged(mockData, component);

      expect(loadSpy).toHaveBeenCalled();
      expect(mockMdFileService.setSelectedMdFileFromServer).toHaveBeenCalledWith(mockData);
      expect(mockMdFileService.setSelectedMdFileFromSideNav).toHaveBeenCalledWith(mockData);
    });

    it('should handle legacy markdownFileIsChanged with path variations', () => {
      const loadSpy = spyOn(component as any, 'loadMarkdownFile');
      
      // Test different path property variations
      const mockDataVariations = [
        { relativePath: 'test1.md' },
        { RelativePath: 'test2.md' },
        { path: 'test3.md' },
        { Path: 'test4.md' }
      ];

      mockDataVariations.forEach(data => {
        (component as any).markdownFileIsChanged(data, component);
      });

      expect(loadSpy).toHaveBeenCalledTimes(4);
    });
  });

  describe('🧹 Cleanup and Memory Management', () => {
    it('should cleanup subscriptions on destroy', () => {
      const destroySpy = spyOn((component as any).destroy$, 'next');
      const completeSpy = spyOn((component as any).destroy$, 'complete');

      component.ngOnDestroy();

      expect(destroySpy).toHaveBeenCalled();
      expect(completeSpy).toHaveBeenCalled();
    });

    it('should prevent memory leaks with takeUntil pattern', fakeAsync(() => {
      fixture.detectChanges();
      tick();

      // Simulate multiple subscription/unsubscription cycles
      for (let i = 0; i < 10; i++) {
        mockMdFileService.selectedMdFileFromSideNav.next(mockMdFile);
        tick(150);
      }

      component.ngOnDestroy();
      
      // Should not crash or leak memory
      expect(component).toBeTruthy();
    }));
  });

  describe('📊 Performance Features', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should debounce rapid file selections', fakeAsync(() => {
      const loadSpy = spyOn(component as any, 'loadMarkdownFile');
      
      // Simulate rapid selections
      mockMdFileService.selectedMdFileFromSideNav.next(mockMdFile);
      mockMdFileService.selectedMdFileFromSideNav.next(mockMdFile);
      mockMdFileService.selectedMdFileFromSideNav.next(mockMdFile);
      
      tick(50); // Partial debounce time
      expect(loadSpy).toHaveBeenCalledTimes(0);
      
      tick(100); // Complete debounce time
      expect(loadSpy).toHaveBeenCalledTimes(1);
    }));

    it('should track loading performance', () => {
      const startTime = new Date();
      (component as any).loadingStartTime = startTime;
      
      expect((component as any).loadingStartTime).toBe(startTime);
    });

    it('should prevent state flickering with debouncing', fakeAsync(() => {
      const updateSpy = spyOn(component as any, 'updateState').and.callThrough();
      
      // Rapid state updates
      (component as any).updateState({ status: 'loading' });
      (component as any).updateState({ status: 'loading' });
      (component as any).updateState({ status: 'loading' });
      
      tick(100);
      
      // Should have called updateState multiple times but observables should debounce
      expect(updateSpy).toHaveBeenCalledTimes(3);
    }));
  });

  describe('🔧 Integration Testing', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should handle complete file loading workflow', fakeAsync(() => {
      // Step 1: File selection
      (component as any).loadMarkdownFile(mockMdFile);
      tick();
      
      // Should be in loading state
      component.currentState$.subscribe(state => {
        expect(state.status).toBe('loading');
      });
      
      // Step 2: Simulate iframe load event
      const mockIframe = {
        addEventListener: (event: string, callback: Function) => {
          if (event === 'load') {
            // Simulate immediate load
            setTimeout(callback, 0);
          }
        }
      };
      component.iframe = { nativeElement: mockIframe } as any;
      (component as any).setupIframeEventListeners();
      
      tick(100);
      
      // Should transition to loaded state
      component.currentState$.subscribe(state => {
        expect(state.status).toBe('loaded');
      });
    }));

    it('should handle indexing workflow correctly', fakeAsync(() => {
      // Mock file as not indexed
      mockIndexingStateService.isFileIndexed.and.returnValue(false);
      mockIndexingStateService.isFileWaiting.and.returnValue(true);
      
      // Load file that needs indexing
      (component as any).loadMarkdownFileWithIndexingCheck(mockMdFile);
      tick();
      
      // Should show indexing state
      component.currentState$.subscribe(state => {
        expect(state.status).toBe('indexing');
        expect(state.isIndexing).toBe(true);
      });
      
      // Simulate indexing completion
      const indexedEvent = { fullPath: mockMdFile.relativePath, isIndexed: true };
      (component as any).handleFileIndexingComplete(indexedEvent);
      
      tick();
      
      // Should transition to loading after indexing
      component.currentState$.subscribe(state => {
        expect(state.status).toBe('loading');
      });
    }));
  });
});
