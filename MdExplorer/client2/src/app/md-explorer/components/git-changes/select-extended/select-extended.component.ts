import { NgContentAst } from '@angular/compiler';
import { Component, ElementRef, Input, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { MatFormField } from '@angular/material/form-field';
import { OptionExtendedComponent } from './option-extended/option-extended.component';

@Component({
  selector: 'app-select-extended',
  templateUrl: './select-extended.component.html',
  styleUrls: ['./select-extended.component.scss']
})
export class SelectExtendedComponent implements OnInit {

  @ViewChild('gitChanges') elGitChanges: MatFormField;
  //@ViewChild(TemplateRef) templateObject: TemplateRef<ContentRef>;

  selectPosition = { x: 0, y: 0, visibility: 'hidden' }
  matIconSpecial: string = 'mat-icon-special'

  constructor(elRef: ElementRef) { }

  SelectChildrensAreVisible: boolean = true;

  ngOnInit(): void {
    //this.templateObject.createEmbeddedView();
  }

  selected = 'volvo';
  setClassOver() {
    this.matIconSpecial = 'mat-icon-special-hover';
  }
  setClassSimple() {
    this.matIconSpecial = 'mat-icon-special';
  }
  setSelectPosition() {
    debugger;
    let test = this.elGitChanges;    
    if (this.SelectChildrensAreVisible) {
      this.selectPosition.visibility = 'visible';
      this.SelectChildrensAreVisible = false;
      this.selectPosition.x = this.elGitChanges._elementRef.nativeElement.offsetLeft;
      this.selectPosition.y = this.elGitChanges._elementRef.nativeElement.offsetTop +
        this.elGitChanges._elementRef.nativeElement.offsetHeight + 44;
    } else {
      this.selectPosition.visibility = 'hidden';
      this.SelectChildrensAreVisible = true;
      this.selectPosition.x = this.elGitChanges._elementRef.nativeElement.offsetLeft;
      this.selectPosition.y = this.elGitChanges._elementRef.nativeElement.offsetTop +
        this.elGitChanges._elementRef.nativeElement.offsetHeight + 44;
    }
  }
}
