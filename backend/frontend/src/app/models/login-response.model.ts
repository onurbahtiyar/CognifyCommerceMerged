export interface LoginResponse {
  token: string;
  expiration: string;
  user: {
    userId: number;
    guid: string;
    username: string;
    email: string;
    firstName: string;
    lastName: string;
    roles: string[];
  };
  isSuccessful: boolean;
  isVerified: boolean;
}
