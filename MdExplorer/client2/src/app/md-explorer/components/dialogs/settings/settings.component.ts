import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { IMdSetting } from '../../../../Models/IMdSetting'
import { AppCurrentMetadataService } from '../../../../services/app-current-metadata.service';
import { MatSnackBar } from '@angular/material/snack-bar';


 
@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss'],
  animations: [  ]
})
export class SettingsComponent implements OnInit {

  _settings: IMdSetting[];
  vscodePath: string;
  jiraServer: string;
  plantumlLocalPath: string;
  javaPath: string;
  localGraphvizDotPath: string;

  constructor(private appCurrentFolder: AppCurrentMetadataService,
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
        this.vscodePath = settings.filter(_ => _.name === "EditorPath")[0].valueString || null;
        this.jiraServer = settings.filter(_ => _.name === "JiraServer")[0].valueString || null;
        this.plantumlLocalPath = settings.filter(_ => _.name === "PlantumlLocalPath")[0].valueString || null;
        this.javaPath = settings.filter(_ => _.name === "JavaPath")[0].valueString || null;
        this.localGraphvizDotPath = settings.filter(_ => _.name === "LocalGraphvizDotPath")[0].valueString || null;
      }
    });
  }

  save() {
    this._settings.filter(_ => _.name === "EditorPath")[0].valueString = this.vscodePath;
    this._settings.filter(_ => _.name === "JiraServer")[0].valueString = this.jiraServer;
    this._settings.filter(_ => _.name === "PlantumlLocalPath")[0].valueString = this.plantumlLocalPath;
    this._settings.filter(_ => _.name === "JavaPath")[0].valueString = this.javaPath;
    this._settings.filter(_ => _.name === "LocalGraphvizDotPath")[0].valueString = this.localGraphvizDotPath;

    // Pass the updated settings to the service
    this.appCurrentFolder.saveSettings(this._settings).subscribe(data => {
      this._snackBar.open("settings saved","" ,{ duration: 1000 });
    });
    this.dialogRef.close(null);
  }

  dismiss() {
    this.dialogRef.close(null);
  }

}
