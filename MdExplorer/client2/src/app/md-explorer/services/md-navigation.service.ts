import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { MdFile } from '../models/md-file';

@Injectable({
  providedIn: 'root'
})
export class MdNavigationService {

  public navigation: MdFile[] = [];
  public navigationGhost: MdFile[] = [];
  public currentIndex: number = 0;

  constructor() {
  }

  public back(): MdFile {
    
    this.navigationGhost.pop();// throw away the item into the copy    
    this.currentIndex = this.navigationGhost.length-1;
    let currentMdFile = this.navigation[this.currentIndex];
    // Navigation back: index " + this.currentIndex
    return currentMdFile;
  }

  public forward(): MdFile {
    
    this.currentIndex = this.currentIndex + 1;
    let currentMdFile = this.navigation[this.currentIndex];  
    this.navigationGhost.push(currentMdFile);
    // Navigation forward: index " + this.currentIndex
    return currentMdFile;
  }

  public resetNavigation() {
    
    this.navigationGhost = [];
    this.navigation = [];
  }

  public setNewNavigation(currentMdFile: MdFile): void {
    // Adding to navigation: " + currentMdFile.fullPath
    if (this.navigationGhost.length>=1) {
      // Checking if current file matches last in navigation
    }
    if (
      (this.navigationGhost.length - 1) >= 0 //check its not at beginning of navigation
      && currentMdFile.fullPath == this.navigationGhost[this.navigationGhost.length - 1].fullPath) {
      // Same file as current, skipping
      return; //DO NOTHING
    }
    this.navigationGhost.push(currentMdFile);
    
    this.navigation = this.deepCopyArray(this.navigationGhost);
    this.currentIndex = this.navigationGhost.length - 1;// index i 0 based, length is 1 based
    // Navigation updated, length: " + this.navigation.length
  }

  /**
   * The slide deck on screen is on this slide (reveal.js's '#/h/v'): the current history entry
   * keeps it, so the title-bar arrows reopen the deck there. Only when the entry is that file —
   * a late message from a deck already left must not land on another entry.
   */
  public rememberSlide(relativePath: string, slideHash: string): void {
    const same = (file: MdFile) => !!file && MdNavigationService.normalize(file.relativePath) === MdNavigationService.normalize(relativePath);
    const current = this.navigationGhost[this.navigationGhost.length - 1];
    if (same(current)) current.slideHash = slideHash;
    // navigation is a copy of navigationGhost: the same entry lives in both.
    const entry = this.navigation[this.currentIndex];
    if (same(entry)) entry.slideHash = slideHash;
  }

  private static normalize(path: string | undefined): string {
    return (path || '').replace(/\\/g, '/').replace(/^\/+/, '').toLowerCase();
  }

  public deepCopyArray<T>(array: T[]): T[] {
    return JSON.parse(JSON.stringify(array));
  }

}
