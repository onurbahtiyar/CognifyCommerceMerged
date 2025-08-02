export interface CategoryAddDto {
  name: string;
  description?: string;
}

export interface CategoryUpdateDto {
  categoryId: number;
  name: string;
  description?: string;
}

export interface CategoryDto {
  categoryId: number;
  name: string;
  description?: string;
}