import { Component, ContentChildren, OnInit, QueryList, ViewChild } from '@angular/core';
import { MatFormField } from '@angular/material/form-field';
import { OptionBranchComponent } from './option-branch/option-branch.component';

@Component({
  selector: 'app-select-branch',
  templateUrl: './select-branch.component.html',
  styleUrls: ['./select-branch.component.scss']
})
export class SelectBranchComponent implements OnInit {

  @ContentChildren(OptionBranchComponent) optionList!: QueryList<OptionBranchComponent>;

  doSearch(searched: string): void {
    this.optionList.forEach(_ => {
      _.searchText = searched;

    });
  }

  constructor() { }

  ngOnInit(): void {
  }
  @ViewChild('gitChanges') elGitChanges: MatFormField;
  SelectChildrensAreVisible: boolean = true;
  selectPosition = { x: 0, y: 0, visibility: 'hidden' }
  matIconSpecial: string = 'mat-icon-special'
  setSelectPosition() {    
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
