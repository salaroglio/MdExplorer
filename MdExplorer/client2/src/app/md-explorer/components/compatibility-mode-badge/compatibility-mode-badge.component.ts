import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CompatibilityModeService } from '../../../services/compatibility-mode.service';
import { CompatibilityMode } from '../../../models/compatibility-mode.model';

@Component({
  selector: 'app-compatibility-mode-badge',
  templateUrl: './compatibility-mode-badge.component.html',
  styleUrls: ['./compatibility-mode-badge.component.scss']
})
export class CompatibilityModeBadgeComponent implements OnInit, OnDestroy {
  currentMode: CompatibilityMode = CompatibilityMode.MdExplorer;
  modeDisplayName = 'MdExplorer';
  modeIcon = 'rocket_launch';
  modeColor = 'primary';

  private destroy$ = new Subject<void>();

  constructor(public compatibilityService: CompatibilityModeService) {}

  ngOnInit(): void {
    // Subscribe to mode changes
    this.compatibilityService.currentMode$
      .pipe(takeUntil(this.destroy$))
      .subscribe(mode => {
        this.currentMode = mode;
        this.updateBadgeProperties();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateBadgeProperties(): void {
    this.modeDisplayName = this.compatibilityService.getModeDisplayName(this.currentMode);
    this.modeIcon = this.compatibilityService.getModeIcon(this.currentMode);
    this.modeColor = this.compatibilityService.getModeColor(this.currentMode);
  }

  /**
   * Get tooltip text based on current mode
   */
  getTooltip(): string {
    switch (this.currentMode) {
      case CompatibilityMode.GitHub:
        return 'GitHub Compatible Mode - Markdown is compatible with GitHub and other standard viewers';
      case CompatibilityMode.CommonMark:
        return 'CommonMark Mode - Strict CommonMark specification compliance';
      case CompatibilityMode.MdExplorer:
      default:
        return 'MdExplorer Mode - All advanced features enabled (PlantUML, interactive emoji, etc.)';
    }
  }

  /**
   * Check if mode is GitHub
   */
  isGitHubMode(): boolean {
    return this.currentMode === CompatibilityMode.GitHub;
  }

  /**
   * Check if mode is MdExplorer
   */
  isMdExplorerMode(): boolean {
    return this.currentMode === CompatibilityMode.MdExplorer;
  }
}
