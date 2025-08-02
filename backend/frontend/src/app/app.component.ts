import { Component, OnInit } from '@angular/core';
import { CookieService } from 'ngx-cookie-service';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';
import { ThemeService } from './services/theme.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  isSidebarOpen = false;
  isLogin = false;
  currentUser: { firstName: string; lastName: string } | null = null;

  constructor(
    private cookieService: CookieService,
    private router: Router,
    private authService: AuthService,
    // ThemeService'i public yaparak şablondan erişim sağlıyoruz
    public themeService: ThemeService 
  ) {}

  ngOnInit(): void {
    // Auth servisine abone olarak login durumunu takip et
    this.authService.isLoggedIn$.subscribe((status) => {
      this.isLogin = status;
      if (status) {
        this.loadUserInfo();
      } else {
        this.currentUser = null;
        this.isSidebarOpen = false; // Çıkış yapıldığında menüyü kapat
      }
    });
  }

  // Kullanıcı bilgilerini cookie'den yükle
  loadUserInfo(): void {
    const userInfo = this.cookieService.get('userInfo');
    if (userInfo) {
      try {
        const parsed = JSON.parse(userInfo);
        this.currentUser = {
          firstName: parsed.FirstName,
          lastName: parsed.LastName
        };
      } catch {
        this.currentUser = null;
      }
    }
  }

  // Mobil menüyü aç/kapat
  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  // Çıkış yap
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}