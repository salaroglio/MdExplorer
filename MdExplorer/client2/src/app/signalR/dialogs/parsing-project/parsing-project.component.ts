import { Component, Inject, OnInit } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-parsing-project',
  templateUrl: './parsing-project.component.html',
  styleUrls: ['./parsing-project.component.scss']
})
export class ParsingProjectComponent implements OnInit {

  public folder$: BehaviorSubject<string>;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dialogRef: MatDialogRef<ParsingProjectComponent>
  ) {
    this.folder$ = data.folder$;
    dialogRef.disableClose = true;
}

  ngOnInit(): void {
  }

}
