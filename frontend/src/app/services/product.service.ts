import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PriceAnalysisResult } from '../models/price-analysis.model';

export interface ProductAddDto {
    name: string;
    description: string;
    price: number;
    stock: number;
    categoryId: number;
    imageUrl: string;
}

export interface ProductDto {
    productId: number;
    name: string;
    description: string;
    price: number;
    stock: number;
    categoryId: number;
    categoryName: string;
    imageUrl: string;
}

export interface TempImageResponse {
    tempImageUrl: string;
}

export interface AttachTempImageRequest {
    tempImageUrl: string;
}

@Injectable({ providedIn: 'root' })
export class ProductService {
    private baseUrl = `${environment.apiUrl}product/`;

    constructor(private http: HttpClient) { }

    addProduct(dto: ProductAddDto): Observable<ApiResponse<ProductDto>> {
        return this.http.post<ApiResponse<ProductDto>>(`${this.baseUrl}add`, dto);
    }

    getAllProducts(): Observable<ApiResponse<ProductDto[]>> {
        return this.http.get<ApiResponse<ProductDto[]>>(`${this.baseUrl}getall`);
    }

    getProductById(id: number): Observable<ApiResponse<ProductDto>> {
        return this.http.get<ApiResponse<ProductDto>>(`${this.baseUrl}get/${id}`);
    }

    updateProduct(dto: ProductDto): Observable<ApiResponse<ProductDto>> {
        return this.http.put<ApiResponse<ProductDto>>(`${this.baseUrl}update`, dto);
    }

    deleteProduct(productId: number): Observable<ApiResponse<any>> {
        return this.http.delete<ApiResponse<any>>(`${this.baseUrl}delete/${productId}`);
    }

    getPriceAnalysis(productName: string): Observable<ApiResponse<PriceAnalysisResult>> {
        const encodedName = encodeURIComponent(productName);
        return this.http.get<ApiResponse<PriceAnalysisResult>>(`${this.baseUrl}priceanalysis/${encodedName}`);
    }

    
    enhanceTempImage(image: File, instruction: string): Observable<ApiResponse<string>> {
        const form = new FormData();
        form.append('image', image);
        form.append('instruction', instruction);
        return this.http.post<ApiResponse<string>>(`${this.baseUrl}enhance-temp-image`, form);
    }

    attachTempImageToProduct(productId: number, tempImageUrl: string): Observable<ApiResponse<string>> {
        const payload: AttachTempImageRequest = { tempImageUrl };
        return this.http.post<ApiResponse<string>>(`${this.baseUrl}attach-image/${productId}`, payload);
    }
}
