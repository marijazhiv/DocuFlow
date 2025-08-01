import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { User } from '../users/users.component';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent {
  user: User | null = null;
  errorMessage: string = '';

  constructor(
    private authService: AuthService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    const username = this.authService.getUsernameFromToken();
    console.log('Username from token:', username);

    if (username) {
      this.userService.getUserByUsername(username).subscribe({
        next: (data) => {
          console.log('Fetched user data:', data);
          this.user = data;
        },
        error: (err) => {
          console.error('Error fetching user:', err);
          this.errorMessage = 'Failed to load user data.';
        }
      });
    } else {
      this.errorMessage = 'User is not logged in.';
    }
  }
}

