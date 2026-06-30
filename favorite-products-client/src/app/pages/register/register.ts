import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormInput } from '../../shared/form-input/form-input';
import { AuthApiService } from '../../core/services/auth-api.service';
import { AuthStore } from '../../core/stores/auth.store';
@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, FormInput],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly authApiService = inject(AuthApiService);
  private readonly authStore = inject(AuthStore);

  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  form = this.fb.group({
    userName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
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
      userName: this.form.value.userName ?? '',
      email: this.form.value.email ?? '',
      password: this.form.value.password ?? ''
    };

    this.authApiService.register(request).subscribe({
    next: response => {
      this.authStore.setAuth(response);
      this.router.navigate(['/products']);
    },
    error: error => {
      console.error(error);
      this.errorMessage.set('שגיאה בהרשמה');
      this.isLoading.set(false);
    }
  });
  }
}