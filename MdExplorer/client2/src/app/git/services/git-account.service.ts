import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { GitAccount, CreateGitAccountRequest, UpdateGitAccountRequest } from '../models/git-account.model';

@Injectable({
  providedIn: 'root'
})
export class GitAccountService {
  private readonly API_BASE = '/api/gitaccount';

  constructor(private http: HttpClient) { }

  /**
   * Gets the Git account configuration for a specific repository
   */
  getAccountForRepository(repositoryPath: string): Observable<GitAccount | null> {
    const params = new HttpParams().set('repositoryPath', repositoryPath);

    return this.http.get<GitAccount>(`${this.API_BASE}/for-repository`, { params }).pipe(
      map(account => {
        // Convert dates from strings to Date objects
        if (account && account.createdAt) {
          account.createdAt = new Date(account.createdAt);
        }
        if (account && account.updatedAt) {
          account.updatedAt = new Date(account.updatedAt);
        }
        return account;
      })
    );
  }

  /**
   * Gets all configured Git accounts
   */
  getAllAccounts(): Observable<GitAccount[]> {
    return this.http.get<GitAccount[]>(this.API_BASE).pipe(
      map(accounts => accounts.map(account => {
        // Convert dates from strings to Date objects
        if (account.createdAt) {
          account.createdAt = new Date(account.createdAt);
        }
        if (account.updatedAt) {
          account.updatedAt = new Date(account.updatedAt);
        }
        return account;
      }))
    );
  }

  /**
   * Creates a new Git account configuration
   */
  createAccount(request: CreateGitAccountRequest): Observable<GitAccount> {
    return this.http.post<GitAccount>(this.API_BASE, request).pipe(
      map(account => {
        if (account.createdAt) {
          account.createdAt = new Date(account.createdAt);
        }
        if (account.updatedAt) {
          account.updatedAt = new Date(account.updatedAt);
        }
        return account;
      })
    );
  }

  /**
   * Updates an existing Git account configuration
   */
  updateAccount(id: string, request: UpdateGitAccountRequest): Observable<GitAccount> {
    return this.http.put<GitAccount>(`${this.API_BASE}/${id}`, request).pipe(
      map(account => {
        if (account.createdAt) {
          account.createdAt = new Date(account.createdAt);
        }
        if (account.updatedAt) {
          account.updatedAt = new Date(account.updatedAt);
        }
        return account;
      })
    );
  }

  /**
   * Deletes a Git account configuration
   */
  deleteAccount(id: string): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(`${this.API_BASE}/${id}`);
  }

  /**
   * Checks if a Git account exists for a specific repository
   */
  hasAccountForRepository(repositoryPath: string): Observable<boolean> {
    const params = new HttpParams().set('repositoryPath', repositoryPath);

    return this.http.get<{ exists: boolean }>(`${this.API_BASE}/exists`, { params }).pipe(
      map(response => response.exists)
    );
  }
}
