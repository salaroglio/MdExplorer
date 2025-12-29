import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
// HttpClientModule is imported in AppModule - DO NOT import here

// Angular Material
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';

// Components
import { GitChatComponent } from './components/git-chat/git-chat.component';

// Providers
import { CHAT_PROVIDER } from './providers/chat-provider.interface';
import { SignalRChatProvider } from './providers/signalr-chat.provider';

@NgModule({
  declarations: [
    GitChatComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    MatTooltipModule
  ],
  exports: [
    GitChatComponent
  ],
  providers: [
    // Provide SignalR implementation (proxies to Firebase via .NET backend)
    // The abstraction layer allows switching providers without changing components
    { provide: CHAT_PROVIDER, useClass: SignalRChatProvider }
  ]
})
export class GitChatModule { }
