import { Component, OnInit } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { NgIf, NgClass } from '@angular/common';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    NgClass,
    NgIf
  ],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.css'
})
export class LayoutComponent implements OnInit {
  isSidebarOpen = true;
  pageTitle = 'Dashboard';
  isAdmin = false;

  constructor(public router: Router, private authService: AuthService) {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      const route = this.router.url.split('/')[1];
      this.pageTitle = route.charAt(0).toUpperCase() + route.slice(1);
    });
  }

  ngOnInit(): void {
    this.checkIfAdmin();

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      const route = this.router.url.split('/')[1];
      this.pageTitle = route.charAt(0).toUpperCase() + route.slice(1);
      this.checkIfAdmin();
    });
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }

  private checkIfAdmin() {
    const role = this.authService.getRoleFromToken();
    console.log('Dohvaćena rola iz tokena:', role);
    this.isAdmin = role === 'Administrator';
  }
}


