export interface LoginDto {
  username: string;
  password: string;
}

export interface RegisterDto {
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
  username?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  profilePictureUrl?: string;
  gender?: boolean;
  address?: string;
  country?: string;
  preferredLanguage?: string;
  twoFactorEnabled?: boolean;
}
