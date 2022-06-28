import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { catchError, map, Observable, share, throwError } from 'rxjs';
import { CookieService } from 'ngx-cookie-service';
import { JwtHelperService } from '@auth0/angular-jwt';
import { asiaUser } from '@shared/models/user.model';
import { LoggerService } from '@shared/components/logging/logger.service';
@Injectable({
  providedIn: 'root'
})
export class AuthorizationService {
  public loggedIn = false;

  private authCookieName = 'authASIA2';
  constructor(
    private http: HttpClient,
    private cookieService: CookieService,
    private jwtHelperService: JwtHelperService,
    private logger: LoggerService
  ) {
    if (this.getToken() !== null) this.loggedIn = true;
    console.log(`Logged in: ${this.loggedIn}`);
  }

  getToken(): string | null {
    if (this.cookieService.check(this.authCookieName)) return this.getTokenIfNotExpired();
    return null;
  }

  deleteToken(): void {
    if (this.cookieService.check(this.authCookieName)) this.cookieService.delete(this.authCookieName);
  }

  private getTokenIfNotExpired(): string | null {
    const token = this.cookieService.get(this.authCookieName);
    if (!this.jwtHelperService.isTokenExpired(token)) return token;
    return null;
  }

  setToken(token: string): void {
    this.cookieService.set(this.authCookieName, token, 1);
  }

  private handleError(error: HttpErrorResponse) {
    if (error.status === 0) return throwError(() => new Error(`An error occured: ${error.error}`));
    else if (error.status === 401) return throwError(() => new Error('Invalid credentials'));
    else return throwError(() => new Error('Something bad happened; please try again later.'));
  }

  getLoggedInUser$: Observable<asiaUser> = this.http.get<asiaUser>(`${environment.aSiaApiServer}/User/loggedIn`).pipe(
    map((res) => asiaUser.copy(res)),
    this.logger.rxjsInfo('[GET AuthorizationService] get loggedInUser'),
    share()
  );

  login(username: string, password: string): Observable<HttpResponse<string>> {
    return this.http
      .post<string>(
        environment.aSiaApiServer + '/User/Authenticate',
        { email: username, password: password },
        { observe: 'response' }
      )
      .pipe(catchError(this.handleError));
  }
}
