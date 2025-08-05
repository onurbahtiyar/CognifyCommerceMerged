export interface ProductReviewDto {
  reviewId: number;
  customerName: string;
  rating: number;
  comment: string;
  createdDate: Date;
  isApproved: boolean;
  productName: string;
  replies: ProductReviewDto[];
}

export interface AdminReplyAddDto {
  parentReviewId: number;
  customerId: number;
  comment: string;
}

export interface ProductReviewAddDto {
  productId: number;
  customerId: number;
  rating: number;
  comment: string;
}