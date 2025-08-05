import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable, EMPTY } from 'rxjs';
import { switchMap, take } from 'rxjs/operators';
import { CookieService } from 'ngx-cookie-service';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private cookieService: CookieService,
    private authService: AuthService
  ) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    
    if (req.url.includes('/user/login') || req.url.includes('/user/register')) {
      return next.handle(req);
    }

    return this.authService.isLoggedIn$.pipe(
      take(1),
      switchMap(isLoggedIn => {
        
        if (isLoggedIn) {
          const token = this.cookieService.get('authToken');
          const clonedReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${token}`
            }
          });
          return next.handle(clonedReq);
        } else {

          console.warn('Kullanıcı giriş yapmamış. API isteği iptal edildi:', req.url);
          return EMPTY;
        }
      })
    );
  }
}