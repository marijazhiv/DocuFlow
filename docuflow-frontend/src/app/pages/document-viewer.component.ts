import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { NgIf } from '@angular/common';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-document-viewer',
  template: `
    <div *ngIf="pdfUrl; else loading" class="pdf-container">
      <iframe [src]="pdfUrl" frameborder="0"></iframe>
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
      flex-grow: 1;
      height: 100%;
    }
    .pdf-container {
      flex-grow: 1;
      height: 100vh; /* popuni ceo vidljivi deo */
    }
    iframe {
      width: 100%;
      height: 100%;
      border: none;
    }
  `]
})
export class DocumentViewerComponent implements OnInit {
  pdfUrl: SafeResourceUrl | null = null;
  documentId!: number;

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient,
    private authService: AuthService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.documentId = +idParam;
        this.loadPdf(this.documentId);
      }
    });
  }

  loadPdf(id: number) {
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
        const url = URL.createObjectURL(blob);
        this.pdfUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
      },
      error: (err) => {
        console.error('Error loading PDF:', err);
      }
    });
  }
}
