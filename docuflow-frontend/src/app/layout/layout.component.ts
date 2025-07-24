import { Component } from '@angular/core';
import {NavigationEnd, Router, RouterOutlet} from "@angular/router";
import {filter} from "rxjs";
import {NgClass} from "@angular/common";

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    NgClass
  ],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.css'
})
export class LayoutComponent {

  isSidebarOpen = true;
  pageTitle = 'Dashboard';

  constructor(public router: Router) {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      const route = this.router.url.split('/')[1];
      this.pageTitle = route.charAt(0).toUpperCase() + route.slice(1);
    });
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }

}
