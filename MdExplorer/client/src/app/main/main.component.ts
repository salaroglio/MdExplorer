import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-main',
  template: `
   <app-navigator></app-navigator>
  `,
  styles: [
  ]
})
export class MainComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
