import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SideNavDataService {

  public currentPath: string;
  public currentName: string;

  constructor() { }
}
