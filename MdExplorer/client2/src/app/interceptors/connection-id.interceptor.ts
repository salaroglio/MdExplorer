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

/**
 * HTTP Interceptor that automatically adds connectionId to all API requests.
 * This ensures that the backend can identify the client and use the correct
 * per-client DatabaseManager context.
 *
 * If the connectionId is not yet available (SignalR still connecting),
 * the request is held until the connectionId arrives.
 */
@Injectable()
export class ConnectionIdInterceptor implements HttpInterceptor {
  constructor(private mdServerMessages: MdServerMessagesService) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    // Only add connectionId to API requests (not external URLs)
    if (!req.url.startsWith('../api/') && !req.url.startsWith('/api/')) {
      return next.handle(req);
    }

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
    return this.mdServerMessages.connectionId$.pipe(
      first(),
      switchMap((id) =>
        next.handle(req.clone({ setParams: { ConnectionId: id } }))
      )
    );
  }
}
