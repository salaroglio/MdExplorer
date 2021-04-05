import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-content',
  template: `
    <p>
      content works!
    </p>
  `,
  styles: [
  ]
})
export class ContentComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
