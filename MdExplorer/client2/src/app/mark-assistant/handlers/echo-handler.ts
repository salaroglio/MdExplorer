import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { MarkInputContext, MarkInputHandler } from '../mark-types';

/**
 * EchoHandler — V0 fallback handler.
 *
 * Always claims the input (lowest priority, runs only if no other handler
 * picked it up). Replies with a static "still learning" message so V0
 * users get a gentle hint that the conversation channel exists but isn't
 * fully wired yet.
 *
 * Replace or override in V3 with an AI-conversation handler.
 */
@Injectable({ providedIn: 'root' })
export class EchoHandler implements MarkInputHandler {
  readonly id = 'echo-fallback';
  readonly priority = 0;

  constructor(private translate: TranslateService) {}

  canHandle(_input: string, _ctx: MarkInputContext): boolean {
    return true;
  }

  handle(_input: string, _ctx: MarkInputContext): Observable<string> {
    // small delay simulates "thinking" — keeps the UX from feeling robotic
    return new Observable<string>(subscriber => {
      firstValueFrom(this.translate.get('MARK.RESPONSE.LEARNING')).then(text => {
        subscriber.next(text);
        subscriber.complete();
      });
    }).pipe(delay(300));
  }
}
