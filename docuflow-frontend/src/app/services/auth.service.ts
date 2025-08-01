import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

interface LoginDto {
  username: string;
  password: string;
}
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = 'https://localhost:7053/api/Auth';

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

  getUsernameFromToken(): string | null {
    const token = localStorage.getItem('token');
    if (!token) return null;

    try {
      const decoded: any = jwtDecode(token);
      console.log('Decoded token:', decoded);
      return decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]
        || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]
        || null;
    } catch (e) {
      console.error('Error decoding token:', e);
      return null;
    }
  }

  getRoleFromToken(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const decoded: any = jwtDecode(token);
      console.log('Decoded token (role):', decoded);
      return decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || null;
    } catch (e) {
      console.error('Error decoding token:', e);
      return null;
    }
  }

  logout() {
    localStorage.removeItem('token');
  }

  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

}

