import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { GitCredential, CreateGitCredentialRequest, UpdateGitCredentialRequest } from '../models/git-credential.model';

@Injectable({
  providedIn: 'root'
})
export class GitCredentialService {
  private readonly API_BASE = '/api/gitaccount/credentials';

  constructor(private http: HttpClient) { }

  /**
   * Gets all Git credentials
   */
  getAllCredentials(): Observable<GitCredential[]> {
    return this.http.get<GitCredential[]>(this.API_BASE).pipe(
      map(credentials => credentials.map(credential => {
        // Convert dates from strings to Date objects
        if (credential.createdAt) {
          credential.createdAt = new Date(credential.createdAt);
        }
        if (credential.updatedAt) {
          credential.updatedAt = new Date(credential.updatedAt);
        }
        return credential;
      }))
    );
  }

  /**
   * Gets a specific Git credential by ID
   */
  getCredential(id: string): Observable<GitCredential> {
    return this.http.get<GitCredential>(`${this.API_BASE}/${id}`).pipe(
      map(credential => {
        if (credential.createdAt) {
          credential.createdAt = new Date(credential.createdAt);
        }
        if (credential.updatedAt) {
          credential.updatedAt = new Date(credential.updatedAt);
        }
        return credential;
      })
    );
  }

  /**
   * Gets credentials by account type (GitHub, GitLab, etc.)
   */
  getCredentialsByType(accountType: string): Observable<{ id: string; username: string; accountName: string }[]> {
    const params = new HttpParams().set('accountType', accountType);

    return this.http.get<{ id: string; username: string; accountName: string }[]>(
      '/api/gitaccount/usernames-by-type',
      { params }
    );
  }

  /**
   * Creates a new Git credential
   */
  createCredential(request: CreateGitCredentialRequest): Observable<GitCredential> {
    return this.http.post<GitCredential>(this.API_BASE, request).pipe(
      map(credential => {
        if (credential.createdAt) {
          credential.createdAt = new Date(credential.createdAt);
        }
        if (credential.updatedAt) {
          credential.updatedAt = new Date(credential.updatedAt);
        }
        return credential;
      })
    );
  }

  /**
   * Updates an existing Git credential
   */
  updateCredential(id: string, request: UpdateGitCredentialRequest): Observable<GitCredential> {
    return this.http.put<GitCredential>(`${this.API_BASE}/${id}`, request).pipe(
      map(credential => {
        if (credential.createdAt) {
          credential.createdAt = new Date(credential.createdAt);
        }
        if (credential.updatedAt) {
          credential.updatedAt = new Date(credential.updatedAt);
        }
        return credential;
      })
    );
  }

  /**
   * Deletes a Git credential
   */
  deleteCredential(id: string): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(`${this.API_BASE}/${id}`);
  }
}
