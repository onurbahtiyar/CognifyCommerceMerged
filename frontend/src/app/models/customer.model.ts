export interface CustomerDto {
  customerId: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  address?: string;
  createdDate: Date;
}

export interface CustomerAddDto {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  address?: string;
}

export interface CustomerUpdateDto {
  customerId: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  address?: string;
}