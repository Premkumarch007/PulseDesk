import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthService } from '../services/aut.service';
import { catchError, Observable, throwError } from 'rxjs';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private readonly authService: AuthService) {}
  intercept(
    req: HttpRequest<any>,
    next: HttpHandler,
  ): Observable<HttpEvent<unknown>> {
    const token = this.authService.getToken();
    if (token)
      req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });

    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status == 401) this.authService.logout();
        return throwError(() => error);
      }),
    );
  }
}
