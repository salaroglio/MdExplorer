import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Branch } from '../models/branch';




@Injectable({
  providedIn: 'root'
})
export class GITService {

  constructor(private http: HttpClient) { }

  getCurrentBranch() {
    const url = '../api/gitservice/branches/feat/getcurrentbranch';
    return this.http.get<Branch>(url);
  }
}
