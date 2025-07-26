import { Component } from '@angular/core';
import { UserService } from '../../services/user.service';
import { LoginDto } from '../../models/user.models';
import { ToastrService } from 'ngx-toastr';
import { ApiResponse } from '../../models/api-response.model';
import { CookieService } from 'ngx-cookie-service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  loginData: LoginDto = { username: '', password: '' };
  loading = false;

  constructor(
    private userService: UserService,
    private toastr: ToastrService,
    private cookieService: CookieService,
    private router: Router
  ) {}

  login() {
    this.loading = true;

    this.userService.login(this.loginData).subscribe({
      next: (res: ApiResponse<any>) => {
        console.log('Login response:', res);
        if (res.success) {
          this.cookieService.set('authToken', res.data.Token, new Date(res.data.Expiration));
          this.cookieService.set('userInfo', JSON.stringify(res.data.User));
          this.toastr.success(res.message || 'Giriş başarılı!', 'Başarılı');
          this.router.navigate(['/dashboard']);
        } else {
          this.toastr.warning(res.message || 'Giriş yapılamadı.', 'Uyarı');
        }
        this.loading = false;
      },
      error: (err) => {
        this.toastr.error(err.error?.message || 'Sunucu hatası.', 'Hata');
        this.loading = false;
      }
    });
  }
}
