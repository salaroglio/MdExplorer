import { Injectable } from '@angular/core';
import { ObservedValueOf } from 'rxjs';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SideNavDataService {

  public currentPath: string;
  public currentName: string;
  private _hideIFrame: BehaviorSubject<boolean>;

  get HideIFrameObservable(): Observable<boolean> {
    return this._hideIFrame.asObservable();
  }

  public SetHideIFrame(data: boolean) {
    this._hideIFrame.next(data);
  }

  constructor() {
    this._hideIFrame = new BehaviorSubject<boolean>(false);
  }
    
}
