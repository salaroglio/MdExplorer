import { Component, OnInit, OnDestroy, ViewChild, ElementRef, HostListener } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, takeUntil } from 'rxjs/operators';
import { SearchService } from '../../services/search.service';
import { SearchResult, FileSearchResult, LinkSearchResult } from '../../Models/search.models';
import { Router } from '@angular/router';
import { MdFileService } from '../../md-explorer/services/md-file.service';
import { ProjectsService } from '../../md-explorer/services/projects.service';

@Component({
  selector: 'app-search-box',
  templateUrl: './search-box.component.html',
  styleUrls: ['./search-box.component.scss']
})
export class SearchBoxComponent implements OnInit, OnDestroy {
  @ViewChild('searchInput') searchInput: ElementRef;
  
  searchControl = new FormControl('');
  searchResults: SearchResult | null = null;
  isSearching = false;
  showResults = false;
  selectedTab: 'files' | 'links' = 'files';
  selectedTabIndex = 0;
  currentProjectPath: string = '';
  
  private destroy$ = new Subject<void>();
  
  constructor(
    private searchService: SearchService,
    private router: Router,
    private mdFileService: MdFileService,
    private projectsService: ProjectsService
  ) {}
  
  ngOnInit(): void {
    
    // Subscribe to current project changes
    this.projectsService.currentProjects$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(project => {
      if (project) {
        this.currentProjectPath = project.path;
      }
    });
    
    // Setup search with debounce
    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(term => {
        if (!term || term.trim().length < 2) {
          this.searchResults = null;
          this.showResults = false;
          return [];
        }
        
        this.isSearching = true;
        this.showResults = true;
        return this.searchService.quickSearch(term);
      }),
      takeUntil(this.destroy$)
    ).subscribe(
      results => {
        
        // Debug: Analizza i file ricevuti per trovare duplicati
        if (results && results.files) {
          
          // Raggruppa per path per trovare duplicati
          const filesByPath = {};
          results.files.forEach((file, index) => {
            const normalizedPath = file.path.toLowerCase();
            if (!filesByPath[normalizedPath]) {
              filesByPath[normalizedPath] = [];
            }
            filesByPath[normalizedPath].push(file);
          });
          
          // Log dei duplicati trovati
          Object.keys(filesByPath).forEach(path => {
            if (filesByPath[path].length > 1) {
              console.warn('[SearchBox] ⚠️ DUPLICATE FILES FOUND!');
              console.warn('[SearchBox] Path:', path);
              console.warn('[SearchBox] Number of duplicates:', filesByPath[path].length);
              console.warn('[SearchBox] Duplicate entries with IDs:', filesByPath[path].map(f => ({
                id: f.id,
                fileName: f.fileName,
                path: f.path
              })));
            }
          });
        }
        
        this.searchResults = results;
        this.isSearching = false;
        
        // Auto-select tab with results
        if (results && results.files.length > 0 && results.links.length === 0) {
          this.selectedTab = 'files';
          this.selectedTabIndex = 0;
        } else if (results && results.links.length > 0 && results.files.length === 0) {
          this.selectedTab = 'links';
          this.selectedTabIndex = 1;
        }
      },
      error => {
        this.isSearching = false;
        this.searchResults = null;
      }
    );
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const clickedElement = event.target as HTMLElement;
    const searchBox = document.querySelector('.search-box-container');
    
    
    // Don't close if clicking on Material Design overlay, tab elements, or toolbar buttons
    if (clickedElement.closest('.cdk-overlay-container') ||
        clickedElement.closest('.mat-tab-label') ||
        clickedElement.closest('.mat-tab-group') ||
        clickedElement.closest('mat-toolbar') ||
        clickedElement.closest('.mat-toolbar')) {
      return;
    }
    
    if (searchBox && !searchBox.contains(clickedElement)) {
      this.showResults = false;
    }
  }
  
  onFocus(): void {
    if (this.searchControl.value && this.searchControl.value.trim().length >= 2) {
      this.showResults = true;
    }
  }
  
  onBlur(event: FocusEvent): void {
    
    // Delay to allow click on results
    setTimeout(() => {
      const searchContainer = document.querySelector('.search-box-container');
      const activeElement = document.activeElement;
      
      if (!(
          (searchContainer && searchContainer.contains(activeElement)) ||
          (activeElement && (
            activeElement.closest('.mat-tab-label') ||
            activeElement.closest('.mat-tab-group') ||
            activeElement.closest('.search-results')
          ))
        )
      });
      
      // Keep results open if focus is still within the search container
      if (searchContainer && searchContainer.contains(activeElement)) {
        return;
      }
      
      // Also keep open if clicking on Material tabs or tab content
      if (activeElement && (
        activeElement.closest('.mat-tab-label') ||
        activeElement.closest('.mat-tab-group') ||
        activeElement.closest('.search-results')
      )) {
        return;
      }
      
      this.showResults = false;
    }, 200);
  }
  
  selectFile(file: FileSearchResult): void {
    
    // Get current project path to calculate relative path
    const currentProject = this.projectsService.currentProjects$.value;
    
    let fullPath = file.path;
    let relativePath = '';
    
    // Calculate relative path from project root
    if (currentProject && currentProject.path) {
      const projectPath = currentProject.path.replace(/\\/g, '/');
      const filePath = file.path.replace(/\\/g, '/');
      
      
      // If file path starts with project path, extract relative part
      if (filePath.startsWith(projectPath)) {
        relativePath = filePath.substring(projectPath.length);
        // Ensure relative path starts with backslash or forward slash
        if (!relativePath.startsWith('/') && !relativePath.startsWith('\\')) {
          relativePath = '\\' + relativePath;
        }
      } else {
        // Fallback: use just the filename with backslash prefix
        relativePath = '\\' + file.fileName;
      }
    } else {
      // No project context, use filename as relative path
      relativePath = '\\' + file.fileName;
    }
    
    
    // Create MdFile object from search result
    const mdFile = {
      fullPath: fullPath,
      relativePath: relativePath,
      path: relativePath, // Add path property for compatibility
      name: file.fileName,
      type: file.fileType || 'mdFile'
    };
    
    
    // Navigate to document
    this.router.navigate(['/main/navigation/document']);
    this.mdFileService.setSelectedMdFileFromSideNav(mdFile as any);
    
    // Clear search and hide results
    this.searchControl.setValue('');
    this.showResults = false;
  }
  
  selectLink(link: LinkSearchResult): void {
    
    // Navigate based on link type
    if (link.fullPath) {
      // Get current project path to calculate relative path
      const currentProject = this.projectsService.currentProjects$.value;
      let relativePath = '';
      
      // Calculate relative path from project root
      if (currentProject && currentProject.path) {
        const projectPath = currentProject.path.replace(/\\/g, '/');
        const filePath = link.fullPath.replace(/\\/g, '/');
        
        // If file path starts with project path, extract relative part
        if (filePath.startsWith(projectPath)) {
          relativePath = filePath.substring(projectPath.length);
          // Ensure relative path starts with backslash or forward slash
          if (!relativePath.startsWith('/') && !relativePath.startsWith('\\')) {
            relativePath = '\\' + relativePath;
          }
        } else {
          // Fallback: use just the filename with backslash prefix
          const fileName = link.fullPath.split('\\').pop() || link.mdTitle || link.path;
          relativePath = '\\' + fileName;
        }
      } else {
        // No project context, use filename as relative path
        const fileName = link.fullPath.split('\\').pop() || link.mdTitle || link.path;
        relativePath = '\\' + fileName;
      }
      
      const mdFile = {
        fullPath: link.fullPath,
        relativePath: relativePath,
        path: relativePath, // Add path property for compatibility
        name: link.mdTitle || link.path,
        type: 'mdFile'
      };
      
      this.router.navigate(['/main/navigation/document']);
      this.mdFileService.setSelectedMdFileFromSideNav(mdFile as any);
    }
    
    // Clear search and hide results
    this.searchControl.setValue('');
    this.showResults = false;
  }
  
  clearSearch(): void {
    this.searchControl.setValue('');
    this.searchResults = null;
    this.showResults = false;
  }
  
  onTabChange(index: number): void {
      newIndex: index,
      previousIndex: this.selectedTabIndex,
      newTab: index === 0 ? 'files' : 'links',
      showResults: this.showResults
    });
    this.selectedTabIndex = index;
    this.selectedTab = index === 0 ? 'files' : 'links';
  }
  
  // Keyboard navigation
  @HostListener('keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      this.showResults = false;
      this.searchInput.nativeElement.blur();
    }
    
    // Add arrow key navigation if needed
  }
  
  // Helper method to get relative path for display
  getRelativePath(fullPath: string): string {
    if (!this.currentProjectPath || !fullPath) {
      return fullPath;
    }
    
    const projectPath = this.currentProjectPath.replace(/\\/g, '/').toLowerCase();
    const filePath = fullPath.replace(/\\/g, '/').toLowerCase();
    
    // If file path starts with project path, extract relative part
    if (filePath.startsWith(projectPath)) {
      let relativePath = fullPath.substring(this.currentProjectPath.length);
      // Remove leading slash/backslash if present
      if (relativePath.startsWith('\\') || relativePath.startsWith('/')) {
        relativePath = relativePath.substring(1);
      }
      return relativePath;
    }
    
    // Fallback: return the full path
    return fullPath;
  }
  
  // Get tooltip text for long paths
  getPathTooltip(fullPath: string): string {
    const relativePath = this.getRelativePath(fullPath);
    // Show full path in tooltip if it's truncated
    return relativePath;
  }
}