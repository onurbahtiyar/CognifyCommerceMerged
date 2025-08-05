import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { CategoryDto, CategoryAddDto, CategoryUpdateDto } from '../models/category.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private baseUrl = `${environment.apiUrl}category/`;

  constructor(private http: HttpClient) {}

  getAllCategories(): Observable<ApiResponse<CategoryDto[]>> {
    return this.http.get<ApiResponse<CategoryDto[]>>(
      `${this.baseUrl}getall`
    );
  }

  getCategoryById(id: number): Observable<ApiResponse<CategoryDto>> {
    return this.http.get<ApiResponse<CategoryDto>>(
      `${this.baseUrl}get/${id}`
    );
  }

  addCategory(dto: CategoryAddDto): Observable<ApiResponse<CategoryDto>> {
    return this.http.post<ApiResponse<CategoryDto>>(
      `${this.baseUrl}add`,
      dto
    );
  }

  updateCategory(dto: CategoryUpdateDto): Observable<ApiResponse<CategoryDto>> {
    return this.http.put<ApiResponse<CategoryDto>>(
      `${this.baseUrl}update`,
      dto
    );
  }

  deleteCategory(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(
      `${this.baseUrl}delete/${id}`
    );
  }
}