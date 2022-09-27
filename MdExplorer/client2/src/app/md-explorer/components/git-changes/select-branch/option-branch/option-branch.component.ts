import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-option-branch',
  templateUrl: './option-branch.component.html',
  styleUrls: ['./option-branch.component.scss']
})
export class OptionBranchComponent implements OnInit {
  @Input() branchName = 'nothing';
  @Input() searchText = 'not searched';

  constructor() { }

  ngOnInit(): void {
  }

}
