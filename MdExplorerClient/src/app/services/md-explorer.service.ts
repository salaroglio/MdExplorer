import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class MdExplorerService {

  constructor(private http: HttpClient) { }

  loadDoc(){
      return this.http.get("https://localhost:5001/MdExplorer/home");

  }
}
