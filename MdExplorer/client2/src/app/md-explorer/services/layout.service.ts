import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LayoutService {
  private sidenavWidthSubject = new BehaviorSubject<number>(240);
  public sidenavWidth$: Observable<number> = this.sidenavWidthSubject.asObservable();

  constructor() { }

  setSidenavWidth(width: number): void {
    this.sidenavWidthSubject.next(width);
  }

  getSidenavWidth(): number {
    return this.sidenavWidthSubject.value;
  }
}