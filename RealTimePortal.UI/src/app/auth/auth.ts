import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { LoginRequest } from './models/login-request';
import { LoginResponse } from './models/login-response';
import { RegisterRequest } from './models/register-request';
import { RegisterResponse } from './models/register-response';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class Auth {

  private readonly tokenKey = 'realTimePortalToken';
  private readonly userNameKey = 'realTimePortalUserName';

  constructor(
    private httpClient: HttpClient
  ) {
  }

  login(
    request: LoginRequest
  ): Observable<LoginResponse> {

    return this.httpClient
      .post<LoginResponse>(
        `${environment.apiUrl}/auth/login`,
        request
      )
      .pipe(
        tap(response => {
          this.setToken(response.accessToken);
          localStorage.setItem(
            this.userNameKey,
            request.userName
          );
        })
      );
  }

  register(
    request: RegisterRequest
  ): Observable<RegisterResponse> {

    return this.httpClient.post<RegisterResponse>(
      `${environment.apiUrl}/auth/register`,
      request
    );
  }

  setToken(token: string): void {
    localStorage.setItem(
      this.tokenKey,
      token
    );
  }

  getToken(): string | null {
    return localStorage.getItem(
      this.tokenKey
    );
  }

  clearToken(): void {
    localStorage.removeItem(
      this.tokenKey
    );
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  getCurrentUserName(): string {
    const claims = this.getTokenClaims();

    return (
      localStorage.getItem(this.userNameKey) ||
      claims['name'] ||
      claims['unique_name'] ||
      claims['preferred_username'] ||
      claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
      'User'
    ) as string;
  }

  getCurrentUserRole(): string {
    const claims = this.getTokenClaims();

    const roleClaim =
      claims['role'] ??
      claims['roles'] ??
      claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

    const role = Array.isArray(roleClaim)
      ? roleClaim[0]
      : roleClaim;

    if (!role) {
      return 'User';
    }

    const normalizedRole = String(role);

    return normalizedRole.toLowerCase() === 'admin'
      ? 'Administrator'
      : normalizedRole;
  }

  getCurrentUserInitials(): string {
    const userName = this.getCurrentUserName().trim();

    if (!userName) {
      return 'U';
    }

    const parts = userName.split(/\s+/).filter(Boolean);

    if (parts.length >= 2) {
      return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
    }

    return userName.substring(0, 1).toUpperCase();
  }

  private getTokenClaims(): Record<string, unknown> {
    const token = this.getToken();

    if (!token) {
      return {};
    }

    try {
      const parts = token.split('.');

      if (parts.length !== 3) {
        return {};
      }

      const base64 = parts[1]
        .replace(/-/g, '+')
        .replace(/_/g, '/');

      const padded = base64.padEnd(
        base64.length + (4 - (base64.length % 4)) % 4,
        '='
      );

      return JSON.parse(atob(padded)) as Record<string, unknown>;
    } catch {
      return {};
    }
  }

  logout(): void {
    this.clearToken();
    localStorage.removeItem(this.userNameKey);
  }
}