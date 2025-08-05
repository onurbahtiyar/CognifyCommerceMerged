import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { ExpenseDto, ExpenseAddDto, ExpenseUpdateDto, ExpenseCategoryDto } from '../models/expense.dto';

@Injectable({
    providedIn: 'root'
})
export class ExpenseService {
    private baseUrl = `${environment.apiUrl}expenses/`;

    constructor(private http: HttpClient) { }

    /** Tüm masrafları listeler */
    getAllExpenses(): Observable<ApiResponse<ExpenseDto[]>> {
        return this.http.get<ApiResponse<ExpenseDto[]>>(`${this.baseUrl}getall`);
    }

    /** Yeni bir masraf ekler */
    addExpense(dto: ExpenseAddDto): Observable<ApiResponse<ExpenseDto>> {
        return this.http.post<ApiResponse<ExpenseDto>>(`${this.baseUrl}add`, dto);
    }

    /** Mevcut bir masrafı günceller */
    updateExpense(dto: ExpenseUpdateDto): Observable<ApiResponse<ExpenseDto>> {
        return this.http.put<ApiResponse<ExpenseDto>>(`${this.baseUrl}update`, dto);
    }

    /** Bir masrafı ID'sine göre siler (soft delete) */
    deleteExpense(expenseId: number): Observable<ApiResponse<any>> {
        return this.http.delete<ApiResponse<any>>(`${this.baseUrl}delete/${expenseId}`);
    }

    /** Formlarda kullanılmak üzere tüm masraf kategorilerini getirir */
    getAllCategories(): Observable<ApiResponse<ExpenseCategoryDto[]>> {
        return this.http.get<ApiResponse<ExpenseCategoryDto[]>>(`${this.baseUrl}getallcategories`);
    }
}