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
    const token = localStorage.getItem('jwtToken');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    return this.http.get<User[]>(this.baseUrl, { headers });
  }

  deleteUser(userId: number, token: string): Observable<any> {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.delete(`${this.baseUrl}/${userId}`, { headers });
  }
  createUser(user: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/create`, user);
  }

  getUserByUsername(username: string): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/username/${username}`);
  }

  changeUserRole(userId: number, newRole: string, token: string) {
    return this.http.put(
      `https://localhost:7053/api/Users/${userId}/role`,
      JSON.stringify(newRole),
      {
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        responseType: 'text'
      }
    );
  }


}

