import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { FormInput } from '../../shared/form-input/form-input';
import { AuthStore } from '../../core/stores/auth.store';
import { LoginRequest } from '../../models/login-request.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, FormInput],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  readonly authStore = inject(AuthStore);

  form = this.fb.group({
    userNameOrEmail: ['', Validators.required],
    password: ['', Validators.required]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request: LoginRequest = {
      userNameOrEmail: this.form.value.userNameOrEmail ?? '',
      password: this.form.value.password ?? ''
    };

    this.authStore.login(request).subscribe({
      next: () => {
        this.router.navigate(['/products']);
      }
    });
  }
}