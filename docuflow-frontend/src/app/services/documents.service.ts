// src/app/services/documents.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Document {
  id: number;
  fileName: string;
  description: string;
  uploadedAt: string;  // timestamp
  uploadedBy: string;  // author
  version: number;
  status: number;
  project?: string;
  documentType?: string;
}

@Injectable({
  providedIn: 'root'
})
export class DocumentsService {
  private apiUrl = 'https://localhost:7053/api/Documents';

  constructor(private http: HttpClient) {}

  getAllDocuments(project?: string): Observable<Document[]> {
    let url = this.apiUrl;
    if (project) {
      url += `?project=${project}`;
    }
    return this.http.get<Document[]>(url);
  }
}

