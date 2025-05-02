import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import {catchError, filter, Observable, switchMap, take, throwError} from 'rxjs';
import { MyAuthService } from './my-auth.service';

@Injectable()
export class MyAuthInterceptorService implements HttpInterceptor {
  constructor(private authService: MyAuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.url == "https://localhost:7291/refresh") {
      //console.log("Bypassing auth token for refresh endpoint");
      return next.handle(req);
    }

    //console.log('Intercepting request to URL:', req.url);

    const token = this.authService.getAuthToken();

    if (token) {
      const isExpired = this.authService.isTokenExpired(token.accessToken);

      if (isExpired) {
        //console.log('Token expired. Refreshing before sending the request...');
        return this.authService.getNewJwt(token).pipe(
          filter((response) => response.accessToken !== null),
          take(1),
          switchMap((response) => {
            const refreshedToken = response.accessToken;
            const clonedRequest = req.clone({
              setHeaders: { Authorization: `Bearer ${refreshedToken}` },
            });
            return next.handle(clonedRequest);
          }),
          catchError((error) => {
            //console.error('Failed to refresh token:', error);
            this.authService.setLoggedIn(null, token.rememberMe);
            return throwError(() => error);
          })
        );
      }
      else {

        const clonedReq = req.clone({
          setHeaders: { Authorization: `Bearer ${token.accessToken}` },
        });

        //console.log('Modified request with Authorization header');
        return next.handle(clonedReq);
      }
    }

    //console.log('Proceeding without Authorization header');
    return next.handle(req);
  }
}
