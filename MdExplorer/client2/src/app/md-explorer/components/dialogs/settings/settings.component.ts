import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { IMdSetting } from '../../../../models/IMdSetting'
import { AppCurrentMetadataService } from '../../../../services/app-current-metadata.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FileChangeNotificationService } from '../../../../services/file-change-notification.service';


 
@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss'],
  animations: [  ]
})
export class SettingsComponent implements OnInit {

  _settings: IMdSetting[];
  vscodePath: string;
  intellijPath: string;
  jiraServer: string;
  plantumlLocalPath: string;
  javaPath: string;
  localGraphvizDotPath: string;

  // File change notification settings
  fileChangeNotificationEnabled: boolean = true;
  isElectronEnvironment: boolean = false;

  constructor(private appCurrentFolder: AppCurrentMetadataService,
    private dialogRef: MatDialogRef<SettingsComponent>,
    private _snackBar: MatSnackBar,
    private fileChangeNotificationService: FileChangeNotificationService
  ) {
    // Check if running in Electron
    this.isElectronEnvironment = !!(window as any).electronAPI?.flashTaskbarIcon;
  }

  ngOnInit(): void {
    this.appCurrentFolder.loadSettings();
    this.appCurrentFolder.settings.subscribe((settings: IMdSetting[]) => {
      if (settings != undefined && settings.length > 0) {
        this._settings = settings;
        this.vscodePath = settings.find(_ => _.name === "EditorPath")?.valueString || null;
        this.intellijPath = settings.find(_ => _.name === "IntelliJPath")?.valueString || null;
        this.jiraServer = settings.find(_ => _.name === "JiraServer")?.valueString || null;
        this.plantumlLocalPath = settings.find(_ => _.name === "PlantumlLocalPath")?.valueString || null;
        this.javaPath = settings.find(_ => _.name === "JavaPath")?.valueString || null;
        this.localGraphvizDotPath = settings.find(_ => _.name === "LocalGraphvizDotPath")?.valueString || null;
      }
    });

    // Load file change notification setting
    this.fileChangeNotificationEnabled = this.fileChangeNotificationService.isEnabled();
  }

  onFileChangeNotificationToggle(): void {
    this.fileChangeNotificationService.setEnabled(this.fileChangeNotificationEnabled);
  }

  save() {
    this.updateSetting("EditorPath", this.vscodePath);
    this.updateSetting("IntelliJPath", this.intellijPath);
    this.updateSetting("JiraServer", this.jiraServer);
    this.updateSetting("PlantumlLocalPath", this.plantumlLocalPath);
    this.updateSetting("JavaPath", this.javaPath);
    this.updateSetting("LocalGraphvizDotPath", this.localGraphvizDotPath);

    // Pass the updated settings to the service
    this.appCurrentFolder.saveSettings(this._settings).subscribe(data => {
      this._snackBar.open("settings saved","" ,{ duration: 1000 });
    });
    this.dialogRef.close(null);
  }

  private updateSetting(name: string, value: string): void {
    const setting = this._settings.find(_ => _.name === name);
    if (setting) {
      setting.valueString = value;
    } else {
      // Create new setting if it doesn't exist
      this._settings.push({ name: name, valueString: value } as IMdSetting);
    }
  }

  dismiss() {
    this.dialogRef.close(null);
  }

}
