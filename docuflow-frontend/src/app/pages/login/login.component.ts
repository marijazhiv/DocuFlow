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
    firstName: '',
    lastName: '',
    username: '',
    password: '',
    role: '',
    profession: ''
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
    this.authService.register(this.signUpObj).subscribe({
      next: (res: any) => {
        if (res && res.message) {
          alert(res.message);
        } else {
          alert('Registration successful!');
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



  /*this.authService.register(registerData).subscribe({
      next: () => {
        alert('Registration successful!');
        this.isSignDivVisiable = false;
      },
      error: (err) => {
        alert('Registration failed: ' + err.error);
      }
    });
  }*/
}

