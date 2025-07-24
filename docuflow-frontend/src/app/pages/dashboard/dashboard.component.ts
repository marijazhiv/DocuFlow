import {Component, inject} from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import {CommonModule} from "@angular/common";
import {DocumentsService} from "../../services/documents.service";

interface DocumentFile {
  id: number;
  name: string;
  description: string;
  timestamp: string;
  author: string;
  version: number;
  status: string;
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  standalone: true,
  imports: [CommonModule]
})
export class DashboardComponent {
  files: DocumentFile[] = [];


  // Umesto constructor, injektuj servise kao polja:
  private documentsService = inject(DocumentsService);
  private authService = inject(AuthService);
  private router = inject(Router);

  isSidebarOpen = true;

  statusMap: { [key: number]: string } = {
    0: 'Draft',
    1: 'WaitingApproval',
    2: 'Approved',
    3: 'ReturnedForEdit'
  };

  ngOnInit() {
    this.loadDocuments();
  }

  loadDocuments() {
    this.documentsService.getAllDocuments().subscribe({
      next: (docs) => {
        this.files = docs.map(doc => ({
          id: doc.id,
          name: doc.fileName,
          description: doc.description,
          timestamp: new Date(doc.uploadedAt).toLocaleDateString(),
          author: doc.uploadedBy,
          version: doc.version,
          status: this.statusMap[doc.status] || 'Unknown'
        }));
      },
      error: (err) => {
        console.error('Error loading documents:', err);
      }
    });
  }
  //constructor(private authService: AuthService, private router: Router) {}

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  goToUsers() {
    this.router.navigate(['/users']);
  }


  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  viewDocument(id: number) {
    this.router.navigate(['/documents', id]); // moraš imati rutu /documents/:id
  }

}


