import { Component, OnInit, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { GitAuthInfo } from '../../models/GitAuthInfo';
import { PullInfo } from '../../models/pullInfo';
import { GITService } from '../../services/gitservice.service';
import { GitMessagesComponent } from '../git-messages/git-messages.component';

@Component({
  selector: 'app-git-auth',
  templateUrl: './git-auth.component.html',
  styleUrls: ['./git-auth.component.scss']
})
export class GitAuthComponent implements OnInit {
  public hide: boolean = true;

  constructor(    
    private dialogRef: MatDialogRef<GitAuthComponent>,
    private gitService: GITService,
    private dialog: MatDialog,) { }

  public dataForCloning: GitAuthInfo = {
    userName: null,
    password: null,
    storeCredentials: true
  }

  ngOnInit(): void {
  }
  useCredentials(): void {
    let pullInfo: PullInfo = {
      UserName: this.dataForCloning.userName,
      Password: this.dataForCloning.password,
      ProjectPath: null,
      BranchName: null
    };
    this.gitService.pull(pullInfo).subscribe(_ => {
      if (_.isConnectionMissing) {
        const dialogRef = this.dialog.open(GitMessagesComponent, {
          width: '300px',
          data: { message: 'Missing connection' }
        });
      }
      if (_.thereAreConflicts) {
        const dialogRef = this.dialog.open(GitMessagesComponent, {
          width: '300px',
          data: { message: 'Conflicts appear, please resolve using Visual Studio Code' }
        });
      }
    });
    this.dialogRef.close(this.dataForCloning);
  }
}
