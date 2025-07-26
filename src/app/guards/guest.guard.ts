import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';

@Injectable({
  providedIn: 'root'
})
export class GuestGuard implements CanActivate {

  constructor(private cookieService: CookieService, private router: Router) {}

  canActivate(): boolean {
    const token = this.cookieService.get('authToken');
    if (!token) {
      return true;
    } else {
      this.router.navigate(['/dashboard']);
      return false;
    }
  }
}
