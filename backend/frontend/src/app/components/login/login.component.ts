import { Component } from '@angular/core';
import { UserService } from '../../services/user.service';
import { LoginDto } from '../../models/user.models';
import { ToastrService } from 'ngx-toastr';
import { ApiResponse } from '../../models/api-response.model';
import { CookieService } from 'ngx-cookie-service';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { ThemeService } from 'src/app/services/theme.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  loginData: LoginDto = { username: 'info@onurbahtiyar.dev', password: '123456' };
  loading = false;
  
  // Footer için dinamik yıl
  currentYear = new Date().getFullYear();

  constructor(
    private userService: UserService,
    private toastr: ToastrService,
    private cookieService: CookieService,
    private router: Router,
    private authService: AuthService,
    public themeService: ThemeService 
  ) {}

  login() {
    this.loading = true;

    this.userService.login(this.loginData).subscribe({
      next: (res: ApiResponse<any>) => {
        if (res.success && res.data?.Token) { // Token varlığını kontrol et
          this.cookieService.set('authToken', res.data.Token, {
             expires: new Date(res.data.Expiration),
             secure: true, // Sadece HTTPS üzerinden gönder
             sameSite: 'Lax'
          });
          this.cookieService.set('userInfo', JSON.stringify(res.data.User));
          
          this.toastr.success(res.message || 'Giriş başarılı!', 'Başarılı');
          this.authService.setLogin();
          this.router.navigate(['/dashboard']);
        } else {
          this.toastr.warning(res.message || 'Kullanıcı adı veya şifre hatalı.', 'Uyarı');
        }
        this.loading = false;
      },
      error: (err) => {
        // API'den gelen hata mesajını kullan, yoksa genel bir mesaj göster.
        const errorMessage = err.error?.message || err.error?.title || 'Sunucuyla iletişim kurulamadı.';
        this.toastr.error(errorMessage, 'Hata');
        this.loading = false;
      }
    });
  }
}