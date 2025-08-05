import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { MarketplaceIntegrationDto, MarketplaceIntegrationAddDto, MarketplaceIntegrationUpdateDto } from '../models/marketplace.model';

@Injectable({
    providedIn: 'root'
})
export class MarketplaceService {
    private baseUrl = `${environment.apiUrl}marketplaceintegrations/`;

    constructor(private http: HttpClient) { }

    getAllIntegrations(): Observable<ApiResponse<MarketplaceIntegrationDto[]>> {
        return this.http.get<ApiResponse<MarketplaceIntegrationDto[]>>(`${this.baseUrl}getall`);
    }

    addIntegration(dto: MarketplaceIntegrationAddDto): Observable<ApiResponse<MarketplaceIntegrationDto>> {
        return this.http.post<ApiResponse<MarketplaceIntegrationDto>>(`${this.baseUrl}add`, dto);
    }

    updateIntegration(dto: MarketplaceIntegrationUpdateDto): Observable<ApiResponse<any>> {
        return this.http.put<ApiResponse<any>>(`${this.baseUrl}update`, dto);
    }
}