import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-option-extended',
  templateUrl: './option-extended.component.html',
  styleUrls: ['./option-extended.component.scss']
})
export class OptionExtendedComponent implements OnInit {
  @Input() title: string;
  constructor() { }

  ngOnInit(): void {
  }

}
