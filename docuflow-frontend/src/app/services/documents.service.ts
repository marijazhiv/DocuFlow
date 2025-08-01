// src/app/services/documents.service.ts
import { Injectable } from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
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

export interface UpdateDocumentStatusDto {
  status?: number;
  commentContent: string;
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

  getAllDocumentsForSearch(params?: any): Observable<Document[]> {
    return this.http.get<Document[]>(`${this.apiUrl}/search/advanced`, { params });
  }

  uploadDocument(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/upload`, formData);
  }

 /* searchAdvanced(params: any) {
    return this.http.get<any[]>(`${this.apiUrl}/search/advanced`, { params });
  }
  searchDocuments(query: string) {
    const params = { query };
    return this.http.get<any[]>(`${this.apiUrl}/search`, { params });
  }

  sortDocuments(sortBy: string, order: string) {
    return this.http.get<any[]>(`${this.apiUrl}/sort`, {
      params: {
        sortBy,
        order
      }
    });
  }
*/

  searchDocuments(params: any) {
    return this.http.get<any[]>(`${this.apiUrl}/search`, { params });
  }

  searchAdvanced(params: any) {
    return this.http.get<any[]>(`${this.apiUrl}/search/advanced`, { params });
  }

  sortDocuments(sortBy: string, order: string) {
    return this.http.get<any[]>(`${this.apiUrl}/sort`, {
      params: { sortBy, order }
    });
  }
  updateDocumentStatus(id: number, dto: UpdateDocumentStatusDto) {
    return this.http.put(`${this.apiUrl}/${id}/status`, dto);
  }
  archiveDocument(documentId: number) {
    return this.http.post(`${this.apiUrl}/${documentId}/archive`, {});
  }





}

