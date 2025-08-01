import { Component } from '@angular/core';
import { FormsModule } from "@angular/forms";
import { CommonModule } from "@angular/common";
import { UserService } from "../../services/user.service";
import { AuthService } from "../../services/auth.service";

export interface User {
  id: number;
  username: string;
  firstName: string;
  lastName: string;
  passwordHash: string;
  passwordSalt: string;
  role: number | string;
  profession: string;
  comments: any[];
}

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    FormsModule,
    CommonModule
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent {
  users: User[] = [];
  filteredUsers: User[] = [];
  searchText: string = '';
  showDialog: boolean = false;
  showEditDialog: boolean = false;

  newUser: any = {
    username: '',
    firstName: '',
    lastName: '',
    password: '',
    role: '',
    profession: ''
  };

  selectedProfession: string = '';
  selectedUser: User | null = null;
  selectedNewRole: string = '';

  userRoles: string[] = ['Author', 'Reviewer', 'Approver', 'Administrator', 'User'];

  professions: string[] = [
    'Software Engineer',
    'Senior Engineer',
    'Tech Lead',
    'Project Manager',
    'HR Lead',
    'QA Engineer',
    'UX/UI Designer',
    'DevOps Engineer',
    'Business Analyst',
    'Product Manager',
    'Other'
  ];

  snackbarMessage: string = '';
  showSnackbar: boolean = false;

  constructor(private userService: UserService, private authService: AuthService) {}

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    const token = localStorage.getItem('jwtToken');
    console.log('JWT token koji se šalje:', token);

    this.userService.getAllUsers().subscribe({
      next: (data) => {
        console.log('Učitani korisnici:', data);
        this.users = data;
        this.filteredUsers = data;
      },
      error: (err) => {
        console.error('Greška pri učitavanju korisnika:', err);
      }
    });
  }

  openDialog() {
    this.showDialog = true;
  }

  closeDialog() {
    this.showDialog = false;
    this.resetNewUser();
  }

  resetNewUser() {
    this.newUser = {
      username: '',
      firstName: '',
      lastName: '',
      password: '',
      role: '',
      profession: ''
    };
  }

  onProfessionChange(): void {
    if (this.selectedProfession !== 'Other') {
      this.newUser.profession = this.selectedProfession;
    } else {
      this.newUser.profession = '';
    }
  }

  createUser() {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailRegex.test(this.newUser.username)) {
      this.showSnackbarMessage('Username must be a valid email address.');
      return;
    }

    if (!this.newUser.password || this.newUser.password.length < 8) {
      this.showSnackbarMessage('Password must be at least 8 characters long.');
      return;
    }

    this.userService.createUser(this.newUser).subscribe({
      next: () => {
        this.loadUsers();
        this.closeDialog();
        this.showSuccessSnackbar('User successfully created.');
      },
      error: (err) => {
        alert('Error creating user: ' + err.error);
      }
    });
  }


  deleteUser(user: User) {
    if (!confirm(`Are you sure you want to delete user ${user.username}?`)) return;

    const token = this.authService.getToken();
    if (!token) {
      this.showSnackbarMessage('Not authorized.');
      return;
    }

    this.userService.deleteUser(user.id, token).subscribe({
      next: () => {
        this.showSnackbarMessage('User deleted successfully.');
        this.filteredUsers = this.filteredUsers.filter(u => u.id !== user.id);
      },
      error: (err) => {
        console.error('Error deleting user:', err);
        this.showSnackbarMessage('Failed to delete user.');
      }
    });
  }

  openEditDialog(user: User) {
    this.selectedUser = user;
    this.selectedNewRole = typeof user.role === 'number' ? this.userRoles[user.role] : user.role;
    this.showEditDialog = true;
  }

  closeEditDialog() {
    this.selectedUser = null;
    this.selectedNewRole = '';
    this.showEditDialog = false;
  }

  confirmRoleChange() {
    if (!this.selectedUser || !this.selectedNewRole) return;

    const token = this.authService.getToken();
    if (!token) {
      this.showSnackbarMessage('Not authorized.');
      return;
    }

    this.userService.changeUserRole(this.selectedUser.id, this.selectedNewRole, token).subscribe({
      next: () => {
        this.showSnackbarMessage('Role updated successfully.');
        this.closeEditDialog();
        this.loadUsers();
      },
      error: (err) => {
        console.error('Error changing role:', err);
        this.showSnackbarMessage('Failed to update role.');
      }
    });
  }

  // === Utility ===

  showSuccessSnackbar(message: string) {
    this.snackbarMessage = message;
    this.showSnackbar = true;
    setTimeout(() => this.showSnackbar = false, 3000);
  }

  showSnackbarMessage(message: string) {
    this.snackbarMessage = message;
    this.showSnackbar = true;
    setTimeout(() => this.showSnackbar = false, 3000);
  }

  ngOnChanges(): void {
    this.filterUsers();
  }

  filterUsers() {
    this.filteredUsers = this.users.filter(u =>
      u.username.toLowerCase().includes(this.searchText.toLowerCase())
    );
  }
}
