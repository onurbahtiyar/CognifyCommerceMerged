import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { CookieService } from 'ngx-cookie-service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private isLoggedInSubject = new BehaviorSubject<boolean>(this.hasToken());
  public isLoggedIn$ = this.isLoggedInSubject.asObservable();

  constructor(
    private cookieService: CookieService,
    private router: Router
  ) {}

  private hasToken(): boolean {
    return !!this.cookieService.get('authToken');
  }

  setLogin(): void {
    this.isLoggedInSubject.next(true);
  }

  logout(): void {
    this.cookieService.delete('authToken', '/');
    this.cookieService.delete('userInfo', '/');

    this.isLoggedInSubject.next(false);
    
    this.router.navigate(['/login']);
  }
}