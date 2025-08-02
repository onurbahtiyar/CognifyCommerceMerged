import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { IResult } from '../models/i-result.model';
import { 
  OrderDto, OrderAddDto, OrderUpdateStatusDto, 
  OrderUpdateShippingDto, ShipperDto 
} from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private orderBaseUrl = `${environment.apiUrl}orders/`;
  private shipperBaseUrl = `${environment.apiUrl}shippers/`;

  constructor(private http: HttpClient) {}

  getAllOrders(): Observable<ApiResponse<OrderDto[]>> {
    return this.http.get<ApiResponse<OrderDto[]>>(`${this.orderBaseUrl}getall`);
  }

  getOrderById(id: number): Observable<ApiResponse<OrderDto>> {
    return this.http.get<ApiResponse<OrderDto>>(`${this.orderBaseUrl}get/${id}`);
  }

  addOrder(dto: OrderAddDto): Observable<ApiResponse<OrderDto>> {
    return this.http.post<ApiResponse<OrderDto>>(`${this.orderBaseUrl}add`, dto);
  }

  updateStatus(dto: OrderUpdateStatusDto): Observable<IResult> {
    return this.http.put<IResult>(`${this.orderBaseUrl}updatestatus`, dto);
  }

  updateShipping(dto: OrderUpdateShippingDto): Observable<IResult> {
    return this.http.put<IResult>(`${this.orderBaseUrl}updateshipping`, dto);
  }

  cancelOrder(id: number): Observable<IResult> {
    return this.http.put<IResult>(`${this.orderBaseUrl}cancel/${id}`, null);
  }

  getAllActiveShippers(): Observable<ApiResponse<ShipperDto[]>> {
    return this.http.get<ApiResponse<ShipperDto[]>>(`${this.shipperBaseUrl}getallactive`);
  }
}