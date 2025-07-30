import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { NgIf } from '@angular/common';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-document-viewer',
  template: `
    <div class="toolbar">
      <i class="fa-solid fa-arrow-left back-icon" (click)="goBack()"></i>
    </div>

    <div *ngIf="type === 'pdf' && fileUrl" class="pdf-container">
      <iframe [src]="fileUrl" frameborder="0"></iframe>
    </div>

    <div *ngIf="type === 'docx' && fileUrl" class="docx-container">
      <iframe [src]="fileUrl" frameborder="0"></iframe>
    </div>

    <div *ngIf="type === 'dwg' && downloadUrl" class="dwg-container">
      <p>Preview nije dostupan za DWG fajlove.</p>
      <a [href]="downloadUrl" target="_blank" download>Download DWG file</a>
    </div>

    <ng-template #loading>
      <p>Loading document...</p>
    </ng-template>
  `,
  standalone: true,
  imports: [NgIf],
  styles: [`
    :host {
      display: flex;
      flex-direction: column;
      height: 100vh;
    }
    .toolbar {
      padding: 10px;
      background-color: #ffffff;
      border-bottom: 1px solid #ddd;
    }
    .back-icon {
      font-size: 20px;
      color: #5c6bc0;
      cursor: pointer;
    }
    .back-icon:hover {
      color: #0056b3;
    }
    .pdf-container, .docx-container, .dwg-container {
      flex-grow: 1;
    }
    iframe {
      width: 100%;
      height: 100%;
      border: none;
    }
  `]
})
export class DocumentViewerComponent implements OnInit {
  fileUrl: SafeResourceUrl | null = null;
  downloadUrl: string | null = null;
  type: string | null = null;
  documentId!: number;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private authService: AuthService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      this.route.queryParamMap.subscribe(queryParams => {
        const typeParam = queryParams.get('type');

        if (idParam && typeParam) {
          this.documentId = +idParam;
          this.type = typeParam.toLowerCase();
          this.loadDocument(this.documentId);
        } else {
          console.error('Missing document id or type in URL.');
        }
      });
    });
  }

  loadDocument(id: number) {
    const token = this.authService.getToken();
    if (!token) {
      console.error('No JWT token found!');
      return;
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http.get(`https://localhost:7053/api/Documents/${id}/stream`, {
      headers,
      responseType: 'blob'
    }).subscribe({
      next: (blob) => {
        this.downloadUrl = URL.createObjectURL(blob);

        if (this.type === 'pdf') {
          this.fileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.downloadUrl);
        } else if (this.type === 'docx') {
          const googleDocsUrl = `https://docs.google.com/gview?url=${encodeURIComponent(this.downloadUrl)}&embedded=true`;
          this.fileUrl = this.sanitizer.bypassSecurityTrustResourceUrl(googleDocsUrl);
        } else {
          // DWG i ostalo – samo link za preuzimanje
          this.fileUrl = null;
        }
      },
      error: (err) => {
        console.error('Error loading document blob:', err);
      }
    });
  }

  goBack() {
    this.router.navigateByUrl('/dashboard');
  }
}

