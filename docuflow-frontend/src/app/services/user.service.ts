import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';

import { Observable } from 'rxjs';
import {User} from "../pages/users/users.component";

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private baseUrl = 'https://localhost:7053/api/Users';

  constructor(private http: HttpClient) {}

  getAllUsers(): Observable<User[]> {
    const token = localStorage.getItem('jwtToken'); // ili gde već čuvaš token
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    return this.http.get<User[]>(this.baseUrl, { headers });
  }

  deleteUser(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  createUser(user: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/create`, user);
  }
}

