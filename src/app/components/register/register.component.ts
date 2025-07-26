import { Component } from '@angular/core';
import { UserService } from '../../services/user.service';
import { RegisterDto } from '../../models/user.models';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  registerData: RegisterDto = { email: '', password: '' };
  loading = false;
  error: string | null = null;
  success: string | null = null;


  constructor(private userService: UserService, private toastr: ToastrService) { }

  register() {
    this.loading = true;
    this.error = null;

    this.userService.register(this.registerData).subscribe({
      next: res => {
        if (res.success) {
          this.toastr.success(res.message, 'Başarılı');
        } else {
          this.toastr.warning(res.message, 'Uyarı');
        }
        this.loading = false;
      },
      error: err => {
        this.toastr.error(err.error?.message || 'Bir hata oluştu.', 'Hata');
        this.loading = false;
      }
    });
  }
}
