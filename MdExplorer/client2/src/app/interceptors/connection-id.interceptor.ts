import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpInterceptor,
  HttpHandler,
  HttpRequest,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { MdServerMessagesService } from '../signalR/services/server-messages.service';

/**
 * HTTP Interceptor that automatically adds connectionId to all API requests.
 * This ensures that the backend can identify the client and use the correct
 * per-client DatabaseManager context.
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

    // Skip if connectionId is not yet available
    if (!connectionId) {
      console.warn('[ConnectionIdInterceptor] connectionId not available yet for request:', req.url);
      return next.handle(req);
    }

    // Add connectionId as query parameter
    const modifiedReq = req.clone({
      setParams: {
        ConnectionId: connectionId,
      },
    });

    return next.handle(modifiedReq);
  }
}
