import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MonitorMDService } from '../../services/monitor-md.service';
import { SideNavDataService } from '../../services/side-nav-data.service';
import { SettingsComponent } from '../settings/settings.component';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss']
})
export class ToolbarComponent implements OnInit {
  TitleToShow: string;

  @Output() toggleSidenav = new EventEmitter<void>();
  constructor(
    private sideNavDataService: SideNavDataService,
    public dialog: MatDialog,
    private monitorMDService: MonitorMDService,
  ) {
    this.TitleToShow = "M↓Explorer";
  }
  private plantumlServer: string;

  openDialog(): void {
    const dialogRef = this.dialog.open(SettingsComponent, {
      width: '250px',
      data: { name: this.plantumlServer }
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed');
      this.plantumlServer = result;
    });
  }
  

  ngOnInit(): void {
    this.monitorMDService.addMdProcessedListener(this.updateModifiedMarkDown, this);
  }

  private updateModifiedMarkDown(data: any, objectThis: any) {
    console.log(data);    
    objectThis.TitleToShow = data.name;
  }

}
