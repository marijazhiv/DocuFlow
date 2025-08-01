import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from "@angular/common";
import { DocumentsService } from "../../services/documents.service";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { DomSanitizer, SafeResourceUrl } from "@angular/platform-browser";

export interface DocumentFile {
    id: number;
    name: string;
    project: string;
    description: string;
    timestamp: string;
    author: string;
    version: number;
    status: string;
    type: string;
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
    groupedFilesByProject: { [projectName: string]: DocumentFile[] } = {};

    private documentsService = inject(DocumentsService);
    private authService = inject(AuthService);
    private router = inject(Router);
    private http = inject(HttpClient);
    private sanitizer = inject(DomSanitizer);

    showDocumentDialog = false;
    dialogFileUrl: SafeResourceUrl | null = null;
    dialogType: string | null = null;

    isSidebarOpen = true;

    statusMap: { [key: number]: string } = {
        0: 'Draft',
        1: 'WaitingApproval',
        2: 'Approved',
        3: 'ReturnedForEdit',
        4: 'Archived'
    };

  snackbarMessage: string = '';
  showSnackbar: boolean = false;

  showSuccessSnackbar(message: string) {
    this.snackbarMessage = message;
    this.showSnackbar = true;

    setTimeout(() => {
      this.showSnackbar = false;
    }, 3000); // automatski nestane posle 3 sekunde
  }
    ngOnInit() {
        this.loadDocuments();
    }

    loadDocuments() {
        this.documentsService.getAllDocuments().subscribe({
            next: (docs) => {
                this.files = docs.map(doc => ({
                    id: doc.id,
                    name: doc.fileName,
                    project: doc.project,
                    description: doc.description,
                    timestamp: new Date(doc.uploadedAt).toLocaleDateString(),
                    author: doc.uploadedBy,
                    version: doc.version,
                    status: this.statusMap[doc.status] || 'Unknown',
                    type: doc.documentType
                })) as DocumentFile[];

                this.groupFilesByProject(this.files);
            },
            error: (err) => {
                console.error('Error loading documents:', err);
            }
        });
    }

    groupFilesByProject(files: DocumentFile[]) {
        this.groupedFilesByProject = {};

        for (const file of files) {
            const project = file.project?.trim() || 'Unassigned';
            if (!this.groupedFilesByProject[project]) {
                this.groupedFilesByProject[project] = [];
            }
            this.groupedFilesByProject[project].push(file);
        }
    }

    groupedFilesKeys(): string[] {
        return Object.keys(this.groupedFilesByProject).sort();
    }

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

    viewDocument(fileId: number, documentType: string) {
        this.router.navigate(['/documents', fileId], { queryParams: { type: documentType } });
    }

    openDocumentDialog(file: DocumentFile) {
        this.dialogType = file.type.toLowerCase();

        const token = this.authService.getToken();
        if (!token) {
            console.error('No JWT token found!');
            return;
        }

        const headers = new HttpHeaders({
            Authorization: `Bearer ${token}`
        });

        this.http.get(`https://localhost:7053/api/Documents/${file.id}/stream`, {
            headers,
            responseType: 'blob'
        }).subscribe({
            next: (blob) => {
                const url = URL.createObjectURL(blob);

                if (this.dialogType === 'pdf') {
                    this.dialogFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
                } else if (this.dialogType === 'docx') {
                    const googleDocsUrl = `https://docs.google.com/gview?url=${encodeURIComponent(url)}&embedded=true`;
                    this.dialogFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(googleDocsUrl);
                } else {
                    this.dialogFileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
                }

                this.showDocumentDialog = true;
            },
            error: (err) => {
                console.error('Error loading document blob:', err);
            }
        });
    }

    closeDocumentDialog() {
        this.showDocumentDialog = false;
        this.dialogFileUrl = null;
        this.dialogType = null;
    }

    archiveDocument(file: DocumentFile) {
        if (confirm(`Are you sure you want to archive "${file.name}"?`)) {
            this.documentsService.archiveDocument(file.id).subscribe({
                next: () => {
                    //alert('Document archived successfully.');
                  this.showSuccessSnackbar('Document archived successfully.');
                    this.loadDocuments();
                },
                error: (err) => {
                    console.error('Error archiving document:', err);
                    alert('Failed to archive document.');
                }
            });
        }
    }

}
