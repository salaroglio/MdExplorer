import {
  Component,
  Input,
  OnChanges,
  OnInit,
  OnDestroy,
  SimpleChanges,
  HostBinding
} from '@angular/core';
import { Subscription } from 'rxjs';
import { Participant } from '../../md-explorer/models/participant';
import { AppCurrentMetadataService } from '../../services/app-current-metadata.service';
import { IMdSetting } from '../../models/IMdSetting';

@Component({
  selector: 'app-participant-gems',
  templateUrl: './participant-gems.component.html',
  styleUrls: ['./participant-gems.component.scss']
})
export class ParticipantGemsComponent implements OnInit, OnChanges, OnDestroy {
  @Input() participants: Participant[] | null | undefined = [];
  @Input() currentUserEmail: string | null = null;
  @Input() maxVisible = 4;
  @Input() size: 'small' | 'medium' = 'small';

  @HostBinding('class.size-medium')
  get sizeMedium(): boolean { return this.size === 'medium'; }

  @HostBinding('class.is-empty')
  get isEmpty(): boolean {
    return !this.teamsChatEnabled ||
           (this.visible.length === 0 && this.overflow.length === 0);
  }

  visible: Participant[] = [];
  overflow: Participant[] = [];
  // The gems only exist to launch Teams, so when the Application setting
  // TeamsChatEnabled is false we hide the whole strip. Default true mirrors
  // the backend default (missing setting → enabled).
  teamsChatEnabled = true;
  private settingsSub: Subscription | null = null;

  constructor(private appMetadataService: AppCurrentMetadataService) {}

  ngOnInit(): void {
    // Kick a load in case nobody else has yet; subsequent open/save cycles of
    // the settings dialog push fresh values through settings$ automatically.
    this.appMetadataService.loadSettings();
    this.settingsSub = this.appMetadataService.settings.subscribe((settings: IMdSetting[]) => {
      if (!settings || settings.length === 0) return;
      const raw = settings.find(s => s.name === 'TeamsChatEnabled')?.valueInt;
      this.teamsChatEnabled = raw == null ? true : raw === 1;
    });
  }

  ngOnDestroy(): void {
    this.settingsSub?.unsubscribe();
  }

  // Fixed palette — keeps gems visually distinct but consistent across the app
  // regardless of light/dark theme. Deterministic per email.
  private readonly palette = [
    '#667eea', '#764ba2', '#f093fb', '#f5576c',
    '#4facfe', '#11998e', '#38ef7d', '#ff9800',
    '#9c27b0', '#00bcd4', '#e91e63', '#3f51b5'
  ];

  ngOnChanges(_: SimpleChanges): void {
    this.rebuild();
  }

  private rebuild(): void {
    const self = (this.currentUserEmail || '').trim().toLowerCase();
    const list = (this.participants || [])
      .filter(p => !!p && !!p.chatEmail)
      .filter(p => !self || (p.chatEmail || '').trim().toLowerCase() !== self);

    if (list.length <= this.maxVisible) {
      this.visible = list;
      this.overflow = [];
    } else {
      // Leave one slot for the +N overflow chip.
      this.visible = list.slice(0, this.maxVisible - 1);
      this.overflow = list.slice(this.maxVisible - 1);
    }
  }

  open(p: Participant, event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    if (!p?.chatEmail) return;
    const url = `msteams://teams.microsoft.com/l/chat/0/0?users=${encodeURIComponent(p.chatEmail)}`;
    const api = (window as any).electronAPI;
    if (api?.openExternal) {
      api.openExternal(url);
      return;
    }
    // Browser fallback — the OS protocol handler picks it up.
    window.location.href = url;
  }

  initials(p: Participant): string {
    const raw = (p.displayName || p.gitName || p.chatEmail || '').trim();
    if (!raw) return '?';
    const parts = raw.split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return (raw[0] + (raw[1] || '')).toUpperCase();
  }

  color(p: Participant): string {
    const key = (p.gitEmail || p.chatEmail || '');
    let hash = 0;
    for (let i = 0; i < key.length; i++) {
      hash = ((hash << 5) - hash + key.charCodeAt(i)) | 0;
    }
    return this.palette[Math.abs(hash) % this.palette.length];
  }

  tooltip(p: Participant): string {
    const name = p.displayName || p.gitName || '';
    return name ? `${name}\n${p.chatEmail}` : p.chatEmail;
  }

  trackByEmail = (_: number, p: Participant) => p.gitEmail || p.chatEmail;
}
