import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, Observable, throwError } from 'rxjs';
import Swal from "sweetalert2";

@Injectable()
export class ServerErrorInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'An unexpected error occurred. Please try again later.';
        let errorCode = 'Error';

        switch (error.status) {
          case 400:
            errorMessage = this.handleBadRequest(error);
            break;

          case 401:
            errorMessage = this.handleUnauthorized(error);
            this.router.navigate(['/login']);
            break;

          case 403:
            errorMessage = 'You do not have permission to perform this action.';
            this.router.navigate(['/unauthorized']);
            break;

          case 404:
            errorMessage = 'The requested resource was not found.';
            break;

          case 422:
            errorMessage = this.handleValidationErrors(error);
            break;

          case 500:
            errorMessage = 'A server error occurred. Please try again later.';
            this.router.navigate(['/error'], {
              queryParams: {
                code: errorCode,
                message: errorMessage,
              },
            });
            return throwError(() => error);

          default:
            console.error('HTTP Error:', errorMessage);
            /*
            this.router.navigate(['/error'], {
              queryParams: {
                code: error.status || errorCode,
                message: errorMessage,
              },
            });
             */

            return throwError(() => error);
        }

        console.error('HTTP Error:', errorMessage);

        /*Swal.fire({
          icon: 'error',
          title: errorCode,
          text: errorMessage,
          confirmButtonText: 'OK',
          timer: 3000, // Automatically close after 3 seconds
          timerProgressBar: true, // Shows a progress bar while counting down
        });*/

        return throwError(() => error);
      })
    );
  }

  private handleBadRequest(error: HttpErrorResponse): string {
    if (error.error && error.error.message) {
      return error.error.message;
    }
    return 'Invalid request. Please check your input.';
  }

  private handleUnauthorized(error: HttpErrorResponse): string {
    if (error.error && error.error.message) {
      return error.error.message;
    }
    return 'Your session has expired. Please log in again.';
  }

  private handleValidationErrors(error: HttpErrorResponse): string {
    if (error.error && error.error.errors) {
      return Object.values(error.error.errors).join(' ');
    }
    return 'There are validation errors. Please check your input.';
  }
}
