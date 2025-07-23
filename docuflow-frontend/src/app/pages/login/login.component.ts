import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common'; // dodaj ovo
import { AuthService } from '../../services/auth.service';
import {Router} from "@angular/router";

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    FormsModule,
    CommonModule // neophodno za *ngIf, *ngClass i ostalo
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
    name: '',
    email: '',
    password: ''
  };

  constructor(private authService: AuthService, private router: Router) {}

  onLogin() {
    const loginData = {
      username: this.loginObj.email,
      password: this.loginObj.password
    };
    console.log('Login data:', loginData);

    this.authService.login(loginData).subscribe({
      next: (token) => {
        console.log('JWT token:', token);  // 👉 ispis tokena u konzoli
        this.authService.saveToken(token);
        alert('Login successful!');
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        alert('Login failed: ' + err.error);
      }
    });
  }


  onRegister() {
    const registerData = {
      username: this.signUpObj.email,
      password: this.signUpObj.password,
      role: 'Author',
      profession: this.signUpObj.name
    };

    this.authService.register(registerData).subscribe({
      next: () => {
        alert('Registration successful!');
        this.isSignDivVisiable = false;
      },
      error: (err) => {
        alert('Registration failed: ' + err.error);
      }
    });
  }
}

