import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SearchResult, SearchRequest, FileSearchResult, LinkSearchResult } from '../Models/search.models';

@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private baseUrl = '../api/search';

  constructor(private http: HttpClient) {
    console.log('[SearchService] Service initialized');
  }

  quickSearch(term: string, maxResults: number = 20): Observable<SearchResult> {
    console.log(`[SearchService] Quick search for: ${term}`);
    
    const params = new HttpParams()
      .set('term', term)
      .set('maxResults', maxResults.toString());
    
    return this.http.get<SearchResult>(`${this.baseUrl}/quick`, { params });
  }

  advancedSearch(request: SearchRequest): Observable<SearchResult> {
    console.log(`[SearchService] Advanced search:`, request);
    
    return this.http.post<SearchResult>(`${this.baseUrl}/advanced`, request);
  }

  searchFiles(term: string, maxResults: number = 50): Observable<{ files: FileSearchResult[], totalFiles: number }> {
    console.log(`[SearchService] Search files for: ${term}`);
    
    const params = new HttpParams()
      .set('term', term)
      .set('maxResults', maxResults.toString());
    
    return this.http.get<{ files: FileSearchResult[], totalFiles: number }>(`${this.baseUrl}/files`, { params });
  }

  searchLinks(term: string, maxResults: number = 50): Observable<{ links: LinkSearchResult[], totalLinks: number }> {
    console.log(`[SearchService] Search links for: ${term}`);
    
    const params = new HttpParams()
      .set('term', term)
      .set('maxResults', maxResults.toString());
    
    return this.http.get<{ links: LinkSearchResult[], totalLinks: number }>(`${this.baseUrl}/links`, { params });
  }
}