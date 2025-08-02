export interface RetailerInfo {
  storeName: string;
  url: string;
}

export interface PriceAnalysisResult {
  minPrice: string;
  maxPrice:string;
  averagePrice: string;
  retailers: RetailerInfo[];
  rawResponse: string;
}