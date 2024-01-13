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
    console.log("back index: " + this.currentIndex);
    console.log("back/navigation: " + this.navigation.length)
    console.log("back/navigationGhost: " + this.navigationGhost.length)
    return currentMdFile;
  }

  public forward(): MdFile {
    
    this.currentIndex = this.currentIndex + 1;
    let currentMdFile = this.navigation[this.currentIndex];  
    this.navigationGhost.push(currentMdFile);
    console.log("forward index: " + this.currentIndex);
    console.log("Forward/navigation: " + this.navigation.length)
    console.log("Forward/navigationGhost: " + this.navigationGhost.length)
    return currentMdFile;
  }

  public resetNavigation() {
    
    this.navigationGhost = [];
    this.navigation = [];
  }

  public setNewNavigation(currentMdFile: MdFile): void {
    console.log("this.navigationGhost.length: " + this.navigationGhost.length);
    console.log("currentMdFile.fullPath: " + currentMdFile.fullPath)
    if (this.navigationGhost.length>=1) {
      console.log(" this.navigationGhost[this.navigationGhost.length - 1].fullPath" + this.navigationGhost[this.navigationGhost.length - 1].fullPath);
      console.log("check: " + (currentMdFile.fullPath == this.navigationGhost[this.navigationGhost.length - 1].fullPath));
    }
    if (
      (this.navigationGhost.length - 1) >= 0 //check its not at beginning of navigation
      && currentMdFile.fullPath == this.navigationGhost[this.navigationGhost.length - 1].fullPath) {
      console.log("RETURN;")
      return; //DO NOTHING
    }
    this.navigationGhost.push(currentMdFile);
    
    this.navigation = this.deepCopyArray(this.navigationGhost);
    this.currentIndex = this.navigationGhost.length - 1;// index i 0 based, length is 1 based
    console.log("setNewNavigation/navigation: " + this.navigation.length)
    console.log("setNewNavigation/navigationGhost: " + this.navigationGhost.length)
  }

  public deepCopyArray<T>(array: T[]): T[] {
    return JSON.parse(JSON.stringify(array));
  }

}
