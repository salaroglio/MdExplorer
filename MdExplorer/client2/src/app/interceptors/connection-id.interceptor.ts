import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpInterceptor,
  HttpHandler,
  HttpRequest,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { switchMap, first } from 'rxjs/operators';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';
import { OpenPerf } from '../shared/open-perf';
import { OPENS_PROJECT, ProjectOpeningGate } from '../shared/project-opening-gate';

/**
 * HTTP Interceptor that automatically adds connectionId to all API requests.
 * This ensures that the backend can identify the client and use the correct
 * per-client DatabaseManager context.
 *
 * If the connectionId is not yet available (SignalR still connecting),
 * the request is held until the connectionId arrives.
 *
 * While a project is opening (ProjectOpeningGate closed) every API request except the
 * SetFolderProject itself is held too, and leaves as soon as the project is ready: the
 * project page is shown at the click, its data requests wait for the service.
 */
@Injectable()
export class ConnectionIdInterceptor implements HttpInterceptor {
  constructor(private mdServerMessages: MdServerMessagesService, private gate: ProjectOpeningGate) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    // Only add connectionId to API requests (not external URLs)
    if (!req.url.startsWith('../api/') && !req.url.startsWith('/api/')) {
      return next.handle(req);
    }

    // Mentre un progetto si apre, la richiesta aspetta che sia pronto (tranne quella che lo apre).
    if (this.gate.isOpening && !req.context.get(OPENS_PROJECT)) {
      OpenPerf.mark(`richiesta TRATTENUTA finché il progetto non è aperto: ${req.url}`);
      this.gate.held(req.url);
      return this.gate.whenOpen().pipe(
        switchMap(() => {
          OpenPerf.mark(`progetto aperto, parte: ${req.url}`);
          return this.withConnectionId(req, next);
        })
      );
    }
    return this.withConnectionId(req, next);
  }

  private withConnectionId(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Skip if connectionId is already in the URL
    if (req.url.includes('connectionId=') || req.url.includes('ConnectionId=')) {
      return next.handle(req);
    }

    const connectionId = this.mdServerMessages.connectionId;

    // If connectionId is already available, attach it immediately
    if (connectionId) {
      return next.handle(
        req.clone({ setParams: { ConnectionId: connectionId } })
      );
    }

    // Wait for connectionId to become available, then attach it
    OpenPerf.mark(`richiesta TRATTENUTA in attesa del ConnectionId di SignalR: ${req.url}`);
    return this.mdServerMessages.connectionId$.pipe(
      first(),
      switchMap((id) => {
        OpenPerf.mark(`ConnectionId arrivato, parte: ${req.url}`);
        return next.handle(req.clone({ setParams: { ConnectionId: id } }));
      })
    );
  }
}
