import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GitRequestKind, GitRequestQueueService } from '../services/git-request-queue.service';

/** The controllers of the service that answer by running git. */
const GIT_API = /\/api\/(WorkingChanges|ModernGit|ModernGitToolbar|RepoSync|gitservice)\//i;

/**
 * What the window asks by itself, on a timer or when something else arrives: the counters of the git
 * bar and the questions to the remote. Nobody is waiting for these with a hand on the mouse.
 */
const ASKED_BY_THE_WINDOW = /\/api\/(WorkingChanges\/list|ModernGit\/remote-status|ModernGitToolbar\/(branch-status|get-data-to-pull)|RepoSync\/(upstream|fetch-all))(\?|$)/i;

/**
 * Sends every request about git through {@link GitRequestQueueService}: three at a time, the same
 * reading never twice. It comes after ConnectionIdInterceptor, so a request held while a project
 * opens does not keep a place in the line.
 */
@Injectable()
export class GitQueueInterceptor implements HttpInterceptor {
  constructor(private queue: GitRequestQueueService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!GIT_API.test(req.url)) return next.handle(req);

    const reading = req.method === 'GET';
    const kind: GitRequestKind = ASKED_BY_THE_WINDOW.test(req.url) ? 'background' : reading ? 'asked' : 'command';
    if (!reading) return this.queue.schedule(null, kind, () => next.handle(req));

    // Same address AND same kind of answer: a text and a JSON of one address are two readings.
    const key = req.responseType + ' ' + req.urlWithParams;
    return this.queue.schedule(key, kind, () => next.handle(req), GitQueueInterceptor.ownCopy);
  }

  /**
   * A merged reading answers several callers (the bar's counters, the changes tab, the commit dialog):
   * each gets its own copy of the data, as when each made its own request.
   */
  private static ownCopy(event: HttpEvent<any>): HttpEvent<any> {
    if (!(event instanceof HttpResponse) || event.body === null || typeof event.body !== 'object') return event;
    if (event.body instanceof Blob || event.body instanceof ArrayBuffer) return event;
    return event.clone({ body: JSON.parse(JSON.stringify(event.body)) });
  }
}
