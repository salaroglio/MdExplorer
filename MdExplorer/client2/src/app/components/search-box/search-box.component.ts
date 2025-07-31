import { Component, OnInit, OnDestroy, ViewChild, ElementRef, HostListener } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, takeUntil } from 'rxjs/operators';
import { SearchService } from '../../services/search.service';
import { SearchResult, FileSearchResult, LinkSearchResult } from '../../models/search.models';
import { Router } from '@angular/router';
import { MdFileService } from '../../md-explorer/services/md-file.service';

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
  
  private destroy$ = new Subject<void>();
  
  constructor(
    private searchService: SearchService,
    private router: Router,
    private mdFileService: MdFileService
  ) {}
  
  ngOnInit(): void {
    console.log('[SearchBox] Component initialized');
    
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
        
        console.log(`[SearchBox] Searching for: ${term}`);
        this.isSearching = true;
        this.showResults = true;
        return this.searchService.quickSearch(term);
      }),
      takeUntil(this.destroy$)
    ).subscribe(
      results => {
        console.log('[SearchBox] Search results received:', results);
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
        console.error('[SearchBox] Search error:', error);
        this.isSearching = false;
        this.searchResults = null;
      }
    );
  }
  
  ngOnDestroy(): void {
    console.log('[SearchBox] Component destroyed');
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const clickedElement = event.target as HTMLElement;
    const searchBox = document.querySelector('.search-box-container');
    
    if (searchBox && !searchBox.contains(clickedElement)) {
      this.showResults = false;
    }
  }
  
  onFocus(): void {
    console.log('[SearchBox] Input focused');
    if (this.searchControl.value && this.searchControl.value.trim().length >= 2) {
      this.showResults = true;
    }
  }
  
  onBlur(): void {
    // Delay to allow click on results
    setTimeout(() => {
      this.showResults = false;
    }, 200);
  }
  
  selectFile(file: FileSearchResult): void {
    console.log('[SearchBox] File selected:', file);
    
    // Create MdFile object from search result
    const mdFile = {
      fullPath: file.path,
      relativePath: file.path.replace(/\\\\/g, '/'),
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
    console.log('[SearchBox] Link selected:', link);
    
    // Navigate based on link type
    if (link.fullPath) {
      const mdFile = {
        fullPath: link.fullPath,
        relativePath: link.fullPath.replace(/\\\\/g, '/'),
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
    console.log('[SearchBox] Clear search');
    this.searchControl.setValue('');
    this.searchResults = null;
    this.showResults = false;
  }
  
  onTabChange(index: number): void {
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
}