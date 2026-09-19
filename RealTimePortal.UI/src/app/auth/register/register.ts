import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';

import { MatFormFieldModule } from '@angular/material/form-field';

import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';

import { Auth } from '../auth';
@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule
  ],
  templateUrl: './register.html'})
export class RegisterComponent {

  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(Auth);
  private readonly router = inject(Router);

  isLoading = false;
  errorMessage = '';
  successMessage = '';

  registerForm = this.fb.nonNullable.group({
    firstName: [
      '',
      [Validators.maxLength(100)]
    ],

    lastName: [
      '',
      [Validators.maxLength(100)]
    ],

    userName: [
      '',
      [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(50)
      ]
    ],

    email: [
      '',
      [
        Validators.required,
        Validators.email
      ]
    ],

    password: [
      '',
      [
        Validators.required,
        Validators.minLength(6)
      ]
    ],

    confirmPassword: [
      '',
      [Validators.required]
    ]
  });

  onRegister(): void {

    this.errorMessage = '';
    this.successMessage = '';

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const value = this.registerForm.getRawValue();

    if (value.password !== value.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.isLoading = true;

    this.authService.register({
      firstName: value.firstName || undefined,
      lastName: value.lastName || undefined,
      userName: value.userName,
      email: value.email,
      password: value.password,
      confirmPassword: value.confirmPassword
    }).subscribe({
      next: () => {
        this.isLoading = false;

        this.successMessage =
          'Registration successful. Redirecting to login...';

        this.registerForm.reset();

        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 1500);
      },

      error: (error) => {
        this.isLoading = false;

        if (error?.error?.errors) {
          const errors = error.error.errors;

          this.errorMessage = Object.values(errors)
            .flat()
            .join(' ');
        } else {
          this.errorMessage =
            error?.error?.message ||
            'Registration failed. Please try again.';
        }
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}