import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms'; 

import {MatButtonModule} from '@angular/material/button'
import {MatCardModule} from '@angular/material/card'
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { Auth } from '../auth'; 

@Component({
  selector: 'app-login',
  imports: [
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './login.html'
})
export class Login {
  userName = '';
  password ='';

  isLoading= false;
  errorMessage='';

  constructor(private auth: Auth, private router:Router)
  {

  }

  login(): void {
  this.errorMessage = '';

  if (!this.userName || !this.password) {
    this.errorMessage = 'Please enter username and password.';
    return;
  }

  this.isLoading = true;

  this.auth.login({
    userName: this.userName,
    password: this.password
  }).subscribe({
    next: () => {
      this.isLoading = false;
      this.router.navigateByUrl('/dashboard');
    },

    error: error => {
      this.isLoading = false;

      if (error.status === 401) {
        this.errorMessage = 'Invalid username or password.';
      } else {
        this.errorMessage =
          'Unable to login. Please try again.';
      }
    }
  });
}

goToRegister(): void {
  this.router.navigate(['/register']);
}
}

