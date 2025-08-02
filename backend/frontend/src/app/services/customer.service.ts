import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { CustomerDto, CustomerAddDto, CustomerUpdateDto } from '../models/customer.model';
import { IResult } from '../models/i-result.model';

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private baseUrl = `${environment.apiUrl}customer/`;

  constructor(private http: HttpClient) {}

  getAllCustomers(): Observable<ApiResponse<CustomerDto[]>> {
    return this.http.get<ApiResponse<CustomerDto[]>>(`${this.baseUrl}getall`);
  }

  getCustomerById(id: number): Observable<ApiResponse<CustomerDto>> {
    return this.http.get<ApiResponse<CustomerDto>>(`${this.baseUrl}get/${id}`);
  }

  addCustomer(dto: CustomerAddDto): Observable<ApiResponse<CustomerDto>> {
    return this.http.post<ApiResponse<CustomerDto>>(`${this.baseUrl}add`, dto);
  }

  updateCustomer(dto: CustomerUpdateDto): Observable<ApiResponse<CustomerDto>> {
    return this.http.put<ApiResponse<CustomerDto>>(`${this.baseUrl}update`, dto);
  }

  deleteCustomer(id: number): Observable<IResult> {
    return this.http.delete<IResult>(`${this.baseUrl}delete/${id}`);
  }
}