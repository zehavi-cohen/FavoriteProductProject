import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { AuthStore } from '../../core/stores/auth.store';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class Header {
  readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);

  logout(): void {
    this.authStore.logout();
    this.router.navigate(['/login']);
  }

  backToAdmin(): void {
    const restored = this.authStore.stopImpersonation();

    if (restored) {
      this.router.navigate(['/admin/users']);
    }
  }
}