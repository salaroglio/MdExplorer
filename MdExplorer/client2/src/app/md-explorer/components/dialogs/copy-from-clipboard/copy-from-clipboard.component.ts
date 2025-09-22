import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MdFile } from '../../../models/md-file';
import { MdFileService } from '../../../services/md-file.service';


@Component({
  selector: 'app-copy-from-clipboard',
  templateUrl: './copy-from-clipboard.component.html',
  styleUrls: ['./copy-from-clipboard.component.scss']
})
export class CopyFromClipboardComponent implements OnInit {
  public imageName: string

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: MdFile,
    private dialogRef: MatDialogRef<CopyFromClipboardComponent>,
    private mdFileService: MdFileService,
    private snackBar: MatSnackBar  ) { }

  ngOnInit(): void {

  }

  dismiss() {
    this.dialogRef.close();
  }
  save() {
    console.log('[CopyFromClipboardComponent] save() called');
    console.log('[CopyFromClipboardComponent] imageName:', this.imageName);
    console.log('[CopyFromClipboardComponent] data:', this.data);

    if (!this.imageName || this.imageName.trim() === '') {
      console.log('[CopyFromClipboardComponent] Image name is empty, showing error');
      this.snackBar.open("Please enter an image name", null, { duration: 3000 });
      return;
    }

    let dataToSend = { fileName: this.imageName, fileInfoNode: this.data };
    console.log('[CopyFromClipboardComponent] Sending data to server:', dataToSend);

    this.mdFileService.pasteFromClipboard(dataToSend).subscribe({
      next: (response) => {
        console.log('[CopyFromClipboardComponent] Server response:', response);
        this.snackBar.open("Image pasted successfully", null, { duration: 5000 });
        this.dialogRef.close();
      },
      error: (error) => {
        console.error('[CopyFromClipboardComponent] Error from server:', error);
        const errorMessage = error?.error?.message || error?.message || 'Failed to paste image';
        this.snackBar.open(errorMessage, null, { duration: 5000 });
      }
    });
  }
}
