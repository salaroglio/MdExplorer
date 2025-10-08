import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LayoutService {
  private sidenavWidthSubject = new BehaviorSubject<number>(240);
  public sidenavWidth$: Observable<number> = this.sidenavWidthSubject.asObservable();

  private sidenavOpenSubject = new BehaviorSubject<boolean>(true);
  public sidenavOpen$: Observable<boolean> = this.sidenavOpenSubject.asObservable();

  constructor() { }

  setSidenavWidth(width: number): void {
    this.sidenavWidthSubject.next(width);
  }

  getSidenavWidth(): number {
    return this.sidenavWidthSubject.value;
  }

  setSidenavOpen(isOpen: boolean): void {
    this.sidenavOpenSubject.next(isOpen);
  }

  getSidenavOpen(): boolean {
    return this.sidenavOpenSubject.value;
  }
}