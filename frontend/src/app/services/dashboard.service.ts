import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { DashboardDto } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private baseUrl = `${environment.apiUrl}dashboard/`;

  constructor(private http: HttpClient) { }

  getDashboardSummary(): Observable<ApiResponse<DashboardDto>> {
    return this.http.get<ApiResponse<DashboardDto>>(`${this.baseUrl}summary`);
  }
}