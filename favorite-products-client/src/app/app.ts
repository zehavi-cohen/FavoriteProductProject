import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';

import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  public readonly auth: AuthService = inject(AuthService);
  private readonly router: Router = inject(Router);

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  backToAdmin(): void {
    const restored = this.auth.stopImpersonation();

    if (restored) {
      this.router.navigate(['/admin/users']);
    }
  }
}