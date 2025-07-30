import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Comment {
  id: number;
  content: string;
  createdAt: string;
  user: {
    id: number;
    username: string;
    firstName: string;
    lastName: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class CommentsService {
  private baseUrl = 'https://localhost:7053/api/Comments';

  constructor(private http: HttpClient) {}

  getCommentsForDocument(documentId: number): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${this.baseUrl}/document/${documentId}`);
  }
}
