import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastr = inject(ToastrService);

  return next(req).pipe(
    catchError((error) => {
      let errorMessage = 'An unexpected error occurred';

      if (error.status === 0) {
        // Network error
        errorMessage = 'Unable to connect to the server. Please check your internet connection.';
      } else if (error.error && error.error.message) {
        // Server returned an error with a message
        errorMessage = error.error.message;
      } else if (error.status) {
        // Other HTTP errors
        errorMessage = `Error ${error.status}: ${error.statusText}`;
      }

      // Show toast notification
      toastr.error(errorMessage, 'Error', {
        timeOut: 5000,
        closeButton: true,
      });

      // Re-throw the error to allow component-level handling
      return throwError(() => error);
    })
  );
};