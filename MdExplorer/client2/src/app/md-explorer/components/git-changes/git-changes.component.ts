import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { MatFormField } from '@angular/material/form-field';


@Component({
  selector: 'app-git-changes',
  templateUrl: './git-changes.component.html',
  styleUrls: ['./git-changes.component.scss']
})
export class GitChangesComponent implements OnInit {
  @ViewChild('gitChanges') elGitChanges: MatFormField;

  selectPosition = { x: 0, y: 0, visibility: 'hidden' }
  matIconSpecial: string = 'mat-icon-special'

  constructor(elRef: ElementRef) { }

  SelectChildrensAreVisible: boolean = true;
  test1: string = 'test1';

  ngOnInit(): void {
  }

  selected = 'volvo';
  setClassOver() {    
    this.matIconSpecial = 'mat-icon-special-hover';
  }
  setClassSimple() {
    this.matIconSpecial = 'mat-icon-special';
  }
  setSelectPosition() {
    let test = this.elGitChanges;    
    if (this.SelectChildrensAreVisible) {
      this.selectPosition.visibility = 'visible';
      this.SelectChildrensAreVisible = false;
      this.selectPosition.x = this.elGitChanges._elementRef.nativeElement.offsetLeft;        
      this.selectPosition.y = this.elGitChanges._elementRef.nativeElement.offsetTop +
        this.elGitChanges._elementRef.nativeElement.offsetHeight +44;
    } else {
      this.selectPosition.visibility = 'hidden';
      this.SelectChildrensAreVisible = true;
      this.selectPosition.x = this.elGitChanges._elementRef.nativeElement.offsetLeft;
      this.selectPosition.y = this.elGitChanges._elementRef.nativeElement.offsetTop +
        this.elGitChanges._elementRef.nativeElement.offsetHeight +44;
    }
    
  }


}
