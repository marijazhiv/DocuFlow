import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

interface LoginDto {
  username: string;
  password: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = 'https://localhost:7053/api/Auth'; // tvoj backend URL

  constructor(private http: HttpClient) {}

  login(data: LoginDto): Observable<string> {
    return this.http.post(this.apiUrl + '/login', data, { responseType: 'text' });
  }

  register(data: any): Observable<any> {
    return this.http.post(this.apiUrl + '/register', data, {  responseType: 'text' });
  }


  saveToken(token: string) {
    localStorage.setItem('token', token);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  logout() {
    localStorage.removeItem('token');
  }


  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }



}

