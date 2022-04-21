import { Injectable } from '@angular/core';
import { ObservedValueOf } from 'rxjs';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SideNavDataService {

  public currentPath: string;
  public currentName: string;

}
