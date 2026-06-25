import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { AdminService } from '../../core/services/admin.service';
import { AdminUser } from '../../models/admin-user.model';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.scss'
})
export class AdminUsers {
  private readonly adminService = inject(AdminService);

  users = signal<AdminUser[]>([]);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.adminService.getUsers().subscribe({
      next: users => {
        this.users.set(users);
        this.isLoading.set(false);
      },
      error: error => {
        console.error(error);
        
          if (error.status === 401 || error.status === 403) {
            this.isLoading.set(false);
            return;
          }
        this.errorMessage.set('שגיאה בטעינת המשתמשים');
        this.isLoading.set(false);
      }
    });
  }

  getRolesText(user: AdminUser): string {
    if (!user.roles || user.roles.length === 0) {
      return '-';
    }

    return user.roles.join(', ');
  }

  isAdmin(user: AdminUser): boolean {
    return user.roles.includes('Admin');
  }
}