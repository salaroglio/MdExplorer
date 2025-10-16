import { Component } from '@angular/core';

@Component({
  selector: 'app-ai-stub',
  templateUrl: './ai-stub.component.html',
  styleUrls: ['./ai-stub.component.scss']
})
export class AiStubComponent {

  openPremiumPage(): void {
    window.open('https://mdexplorer.net/ai-premium', '_blank');
  }

  enterLicenseKey(): void {
    // TODO: Implement license activation dialog
    alert('License activation coming soon!');
  }
}
