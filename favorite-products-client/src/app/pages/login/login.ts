import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  form = this.fb.group({
    userNameOrEmail: ['', Validators.required],
    password: ['', Validators.required]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const request = {
      userNameOrEmail: this.form.value.userNameOrEmail ?? '',
      password: this.form.value.password ?? ''
    };

    this.authService.login(request).subscribe({
      next: response => {
        this.isLoading.set(false);

        if (response.roles.includes('Admin')) {
          this.router.navigate(['/admin/users']);
        } else {
          this.router.navigate(['/products']);
        }
      },
      error: error => {
        this.isLoading.set(false);
        console.error(error);
        this.errorMessage.set('שם משתמש או סיסמה שגויים');
      }
    });
  }
}