import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { IMdSetting } from '../../../Models/IMdSetting'
import { AppCurrentFolderService } from '../../../services/app-current-folder.service';
import { MatSnackBar } from '@angular/material/snack-bar';


@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {

  _settings: IMdSetting[];
  plantumlServer: string;
  jiraServer: string;

  constructor(private appCurrentFolder: AppCurrentFolderService,
    private dialogRef: MatDialogRef<SettingsComponent>,
    private _snackBar: MatSnackBar
  ) {

  }

  ngOnInit(): void {
    this.appCurrentFolder.loadSettings();
    this.appCurrentFolder.settings.subscribe((data: any) => {
      var settings = data.settings as IMdSetting[];
      if (settings != undefined) {
        this._settings = settings;
        this.plantumlServer = settings.filter(_ => _.name === "PlantumlServer")[0].valueString || null;
        this.jiraServer = settings.filter(_ => _.name === "JiraServer")[0].valueString || null;
      }
    });
  }

  save() {
    this._settings.filter(_ => _.name === "PlantumlServer")[0].valueString = this.plantumlServer;
    this._settings.filter(_ => _.name === "JiraServer")[0].valueString = this.jiraServer;
    this.appCurrentFolder.saveSettings().subscribe(data => {
      this._snackBar.open("settings saved","" ,{ duration: 1000 });
    });
    this.dialogRef.close(null);
  }

  dismiss() {
    this.dialogRef.close(null);
  }

}
