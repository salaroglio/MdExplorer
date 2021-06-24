import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { IMdSetting } from '../../../Models/IMdSetting'
import { AppCurrentFolderService } from '../../../services/app-current-folder.service';


@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {

  plantumlServer: string;
  jiraServer: string;

  constructor(private appCurrentFolder: AppCurrentFolderService,
    private dialogRef: MatDialogRef<SettingsComponent>
  ) {

  }

  ngOnInit(): void {    
    this.appCurrentFolder.loadSettings();
    this.appCurrentFolder.settings.subscribe((data:any) => {
      var settings = data.settings as IMdSetting[];
      this.plantumlServer = settings.filter(_ => _.name === "PlantumlServer")[0].valueString || null;
    });
  }

  save() {

  }

  dismiss() {

  }

}
