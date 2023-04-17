import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-git-messages',
  templateUrl: './git-messages.component.html',
  styleUrls: ['./git-messages.component.scss']
})
export class GitMessagesComponent implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public data: any,) { }

  ngOnInit(): void {
  }

}
