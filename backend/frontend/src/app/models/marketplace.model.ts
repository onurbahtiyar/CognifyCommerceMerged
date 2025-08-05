export interface MarketplaceIntegrationDto {
    id: number;
    marketplaceName: string;
    apiKey: string;
    apiSecret?: string;
    apiUrl?: string;
    description?: string;
    isActive: boolean;
}

export interface MarketplaceIntegrationAddDto {
    marketplaceName: string;
    apiKey: string;
    apiSecret?: string;
    apiUrl?: string;
    description?: string;
    isActive: boolean;
}

export interface MarketplaceIntegrationUpdateDto {
    id: number;
    marketplaceName: string;
    apiKey: string;
    apiSecret?: string;
    apiUrl?: string;
    description?: string;
    isActive: boolean;
}