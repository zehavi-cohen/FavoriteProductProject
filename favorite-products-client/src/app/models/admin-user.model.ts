export interface AdminUser {
  userId: number;
  userName: string;
  email: string;
  isActive: boolean;
  roles: string[];
}