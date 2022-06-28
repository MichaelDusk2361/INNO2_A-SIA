import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { AuthorizationService } from '@shared/backend/authorization.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private authorizationService: AuthorizationService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((err) => {
        if (err.status === 401) {
          // auto logout if 401 response returned from api
          this.authorizationService.loggedIn = false;
          this.authorizationService.deleteToken();
          //   location.reload(); // find solution (route to login)
        }

        const error = err.error.message || err.statusText;
        return throwError(() => error);
      })
    );
  }
}
