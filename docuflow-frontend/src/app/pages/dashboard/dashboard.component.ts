import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import {CommonModule} from "@angular/common";

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  standalone: true,
  imports: [CommonModule]
})
export class DashboardComponent {
  isSidebarOpen = true;
  files = [
    {
      name: 'Report.pdf',
      description: 'Monthly financial report',
      timestamp: '2025-07-22 14:30',
      author: 'Marija Živanović',
      version: 'v1.1',
      status: 'Active'
    },
    {
      name: 'DraftProposal.docx',
      description: 'Initial draft for new proposal',
      timestamp: '2025-07-18 10:15',
      author: 'Nikola Petrović',
      version: 'v0.9',
      status: 'Draft'
    },
    {
      name: 'DesignArchive.png',
      description: 'Previous version of design elements',
      timestamp: '2025-06-30 09:00',
      author: 'Ivana Milić',
      version: 'v2.0',
      status: 'Archived'
    },
    {
      name: 'DesignArchive.png',
      description: 'Previous version of design elements',
      timestamp: '2025-06-30 09:00',
      author: 'Ivana Milić',
      version: 'v2.0',
      status: 'Archived'
    }
  ];



  constructor(private authService: AuthService, private router: Router) {}

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}


