export interface AuthResponse {
  userId: number;
  userName: string;
  email: string;
  roles: string[];
  token: string;

  isImpersonating: boolean;
  impersonatedByUserId: number | null;
  impersonatedByUserName: string | null;
}