import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { P2PService } from '../../../../services/p2p.service';

@Component({
  selector: 'app-p2p-status-widget',
  templateUrl: './p2p-status-widget.component.html',
  styleUrls: ['./p2p-status-widget.component.scss']
})
export class P2PStatusWidgetComponent implements OnInit, OnDestroy {
  isAvailable = false;
  totalPeers = 0;
  isUploading = false;
  lastUploadSpeed = 0;
  lastTorrentName = '';

  private subscriptions: Subscription[] = [];
  private uploadTimeout: any;

  constructor(
    private p2pService: P2PService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    // Check if P2P is available
    this.subscriptions.push(
      this.p2pService.isAvailable$.subscribe(available => {
        this.isAvailable = available;
      })
    );

    // Listen for peer connections
    this.subscriptions.push(
      this.p2pService.peerConnected$.subscribe(data => {
        this.totalPeers = data.numPeers;
        // Show snackbar notification
        this.snackBar.open(
          `Peer connesso a "${data.torrentName}"`,
          'OK',
          {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'bottom',
            panelClass: ['p2p-snackbar']
          }
        );
      })
    );

    // Listen for upload activity
    this.subscriptions.push(
      this.p2pService.uploadActivity$.subscribe(data => {
        this.totalPeers = data.numPeers;
        this.lastUploadSpeed = data.uploadSpeed;
        this.lastTorrentName = data.torrentName;
        this.isUploading = data.uploadSpeed > 0;

        // Clear uploading state after 10s of no activity
        if (this.uploadTimeout) {
          clearTimeout(this.uploadTimeout);
        }
        this.uploadTimeout = setTimeout(() => {
          this.isUploading = false;
        }, 10000);
      })
    );

    // Listen for total peers from transfers
    this.subscriptions.push(
      this.p2pService.totalPeers$.subscribe(peers => {
        if (peers > 0) {
          this.totalPeers = peers;
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(s => s.unsubscribe());
    if (this.uploadTimeout) {
      clearTimeout(this.uploadTimeout);
    }
  }

  formatSpeed(bytesPerSecond: number): string {
    return this.p2pService.formatSpeed(bytesPerSecond);
  }

  getTooltip(): string {
    if (!this.isAvailable) {
      return 'P2P non disponibile';
    }
    if (this.isUploading) {
      return `Uploading "${this.lastTorrentName}" a ${this.formatSpeed(this.lastUploadSpeed)} - ${this.totalPeers} peer`;
    }
    if (this.totalPeers > 0) {
      return `${this.totalPeers} peer connessi`;
    }
    return 'P2P attivo - nessun peer connesso';
  }
}
