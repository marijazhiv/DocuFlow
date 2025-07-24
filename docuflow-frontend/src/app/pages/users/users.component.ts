import { Component } from '@angular/core';
import {FormsModule} from "@angular/forms";
import {CommonModule} from "@angular/common";
import {UserService} from "../../services/user.service";
/*interface User {
  username: string;
  firstName: string;
  lastName: string;
  role: string;
  profession: string;
}*/

export interface User {
  id: number;
  username: string;
  firstName: string;
  lastName: string;
  passwordHash: string;
  passwordSalt: string;
  role: number;
  profession: string;
  comments: any[]; // ili preciznije ako imaš definisan model za komentar
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

  newUser: any = {
    username: '',
    firstName: '',
    lastName: '',
    password: '',
    role: '',
    profession: ''
  };

  constructor(private userService: UserService) {}

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    const token = localStorage.getItem('jwtToken');
    console.log('📦 JWT token koji se šalje:', token);

    this.userService.getAllUsers().subscribe({
      next: (data) => {
        console.log('✅ Učitani korisnici:', data);
        this.users = data;
        this.filteredUsers = data;
      },
      error: (err) => {
        console.error('❌ Greška pri učitavanju korisnika:', err);
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

  createUser() {
    this.userService.createUser(this.newUser).subscribe({
      next: () => {
        this.loadUsers();
        this.closeDialog();
      },
      error: (err) => {
        alert('Error creating user: ' + err.error);
      }
    });
  }

  deleteUser(user: User) {
    this.userService.deleteUser(user.id).subscribe(() => {
      this.loadUsers();
    });
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
