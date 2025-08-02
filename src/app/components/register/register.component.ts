import { Component, ViewChild } from '@angular/core';
import { UserService } from '../../services/user.service';
import { RegisterDto } from '../../models/user.models';
import { ToastrService } from 'ngx-toastr';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  @ViewChild('registerForm') registerForm!: NgForm;

  // Form adımlarını yönetmek için
  currentStep = 1;

  // Tam RegisterDto modelini burada başlatıyoruz
  registerData: RegisterDto = {
    email: '',
    password: '',
    username: '',
    firstName: '',
    lastName: '',
    phoneNumber: '',
    dateOfBirth: undefined,
    gender: true,
    address: '',
    country: '',
    preferredLanguage: 'tr',
  };

  loading = false;
  currentYear = new Date().getFullYear();

  constructor(
    private userService: UserService, 
    private toastr: ToastrService,
    private router: Router
  ) { }

  // Sonraki adıma geç
  nextStep() {
    if (this.currentStep < 3) {
      this.currentStep++;
    }
  }

  // Önceki adıma dön
  prevStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }
  
  // Mevcut adımın geçerli olup olmadığını kontrol et
  isCurrentStepInvalid(): boolean {
    const controls = this.registerForm?.controls;
    if (!controls) return true;

    if (this.currentStep === 1) {
      return controls['email']?.invalid || controls['username']?.invalid || controls['password']?.invalid;
    }
    if (this.currentStep === 2) {
       return controls['firstName']?.invalid || controls['lastName']?.invalid;
    }
    // Diğer adımlar için de benzer kontroller eklenebilir.
    return false;
  }


  // Formu gönder
  register() {
    if (this.registerForm.invalid) {
      this.toastr.warning('Lütfen tüm zorunlu alanları doldurun.', 'Uyarı');
      return;
    }
    this.loading = true;

    this.userService.register(this.registerData).subscribe({
      next: res => {
        if (res.success) {
          this.toastr.success(res.message || 'Hesabınız başarıyla oluşturuldu! Lütfen giriş yapın.', 'Başarılı');
          this.router.navigate(['/login']);
        } else {
          this.toastr.warning(res.message, 'Uyarı');
        }
        this.loading = false;
      },
      error: err => {
        this.toastr.error(err.error?.message || 'Kayıt sırasında bir hata oluştu.', 'Hata');
        this.loading = false;
      }
    });
  }
}