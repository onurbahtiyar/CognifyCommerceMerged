import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoginDto, RegisterDto } from '../models/user.models';
import { ApiResponse } from '../models/api-response.model';
import { LoginResponse } from '../models/login-response.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private http: HttpClient) { }

  login(payload: LoginDto): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(environment.apiUrl + 'user/login', payload);
  }

  register(payload: RegisterDto): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<any>>(environment.apiUrl + 'user/register', payload);
  }
}
