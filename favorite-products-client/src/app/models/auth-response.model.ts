export interface AuthResponse {
  userId: number;
  userName: string;
  email: string;
  roles: string[];
  token: string;
}