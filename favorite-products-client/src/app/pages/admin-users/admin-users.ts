import { Component, inject } from '@angular/core';

import { AdminUsersStore } from './admin-users.store';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [],
  providers: [AdminUsersStore],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.scss'
})
export class AdminUsers {
  readonly store = inject(AdminUsersStore);

  ngOnInit(): void {
    this.store.loadUsers();
  }
}