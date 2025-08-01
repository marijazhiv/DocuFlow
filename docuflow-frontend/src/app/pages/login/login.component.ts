import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import {Router} from "@angular/router";

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    FormsModule,
    CommonModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  isSignDivVisiable = false;

  loginObj = {
    email: '',
    password: ''
  };

  signUpObj = {
    firstName: '',
    lastName: '',
    username: '',
    password: '',
    role: '',
    profession: ''
  };

  snackbarMessage: string = '';
  showSnackbar: boolean = false;

  showSuccessSnackbar(message: string) {
    this.snackbarMessage = message;
    this.showSnackbar = true;

    setTimeout(() => {
      this.showSnackbar = false;
    }, 3000);
  }

  constructor(private authService: AuthService, private router: Router) {}

  onLogin() {
    const loginData = {
      username: this.loginObj.email,
      password: this.loginObj.password
    };
    console.log('Login data:', loginData);

    this.authService.login(loginData).subscribe({
      next: (token) => {
        console.log('JWT token:', token);
        this.authService.saveToken(token);

        this.showSuccessSnackbar('Login successful!');
        setTimeout(() => {
          this.router.navigate(['/dashboard']);
        }, 1500);
      },
      error: (err) => {
        alert('Login failed: ' + err.error);
      }
    });
  }


  selectedProfession: string = '';

  onProfessionChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    if (value !== 'Other') {
      this.signUpObj.profession = value;
    } else {
      this.signUpObj.profession = '';
    }
  }

  onRegister() {
    if (!this.validateRegistrationInput()) return;

    this.authService.register(this.signUpObj).subscribe({
      next: (res: any) => {
        if (res && res.message) {
          alert(res.message);
        } else {
          this.showSuccessSnackbar('Registration successful!');
        }
        this.isSignDivVisiable = false;
      },
      error: (err) => {
        let errorMessage = 'Registration failed';

        if (err.error && err.error.error) {
          errorMessage = err.error.error;
        } else if (typeof err.error === 'string') {
          errorMessage = err.error;
        }

        alert(errorMessage);
      }
    });
  }
  validateRegistrationInput(): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailRegex.test(this.signUpObj.username)) {
      this.showSuccessSnackbar('Username must be a valid email address.');
      return false;
    }

    if (this.signUpObj.password.length < 8) {
      this.showSuccessSnackbar('Password must be at least 8 characters long.');
      return false;
    }

    if (!this.signUpObj.firstName || !this.signUpObj.lastName) {
      this.showSuccessSnackbar('Please fill in your first and last name.');
      return false;
    }

    if (!this.signUpObj.role) {
      this.showSuccessSnackbar('Please select a role.');
      return false;
    }

    if (!this.signUpObj.profession) {
      this.showSuccessSnackbar('Please select or enter a profession.');
      return false;
    }

    return true;
  }


}

