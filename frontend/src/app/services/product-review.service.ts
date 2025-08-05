import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { IResult } from '../models/i-result.model';
import { ProductReviewDto, AdminReplyAddDto, ProductReviewAddDto } from '../models/product-review.model';

@Injectable({
  providedIn: 'root'
})
export class ProductReviewService {
  private baseUrl = `${environment.apiUrl}productreviews/`;

  constructor(private http: HttpClient) { }

  getUnapprovedReviews(): Observable<ApiResponse<ProductReviewDto[]>> {
    return this.http.get<ApiResponse<ProductReviewDto[]>>(`${this.baseUrl}getunapproved`);
  }

  approveReview(reviewId: number): Observable<IResult> {
    return this.http.put<IResult>(`${this.baseUrl}approve/${reviewId}`, null);
  }

  addAdminReply(dto: AdminReplyAddDto): Observable<IResult> {
    return this.http.post<IResult>(`${this.baseUrl}addadminreply`, dto);
  }

  deleteReview(reviewId: number): Observable<IResult> {
    return this.http.delete<IResult>(`${this.baseUrl}delete/${reviewId}`);
  }

  addReview(dto: ProductReviewAddDto): Observable<IResult> {
    return this.http.post<IResult>(`${this.baseUrl}add`, dto);
  }

  getAllReviews(): Observable<ApiResponse<ProductReviewDto[]>> {
    return this.http.get<ApiResponse<ProductReviewDto[]>>(`${this.baseUrl}getall`);
  }
}