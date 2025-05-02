import { Injectable } from '@angular/core';
import {HttpClient, HttpErrorResponse} from '@angular/common/http';
import {MyConfig} from "../../my-config";
import {finalize, Observable, tap} from 'rxjs';
import {jwtDecode} from "jwt-decode";

export interface LoginRequest {
  usernameOrEmail: string;
  password: string,
  rememberMe: boolean
}

export interface LoginResponse {
  accessToken: string,
  refreshToken: string
}

export interface AuthTokenInfo {
  accessToken: string;
  refreshToken: string;
  nameid: number;
  unique_name: string;
  email: string;
  Role: string;
  rememberMe: boolean
}

@Injectable({ providedIn: 'root' })
export class MyAuthService {
  private apiUrl = `${MyConfig.api_address}`;
  private refreshingToken = false;

  constructor(private http: HttpClient) {}

  getCurrentUser(): AuthTokenInfo | null {
    const authToken = this.getAuthToken();
    if (authToken) {
      return {
        nameid: authToken.nameid,
        unique_name: authToken.unique_name,
        email: authToken.email,
        accessToken: authToken.accessToken,
        refreshToken: authToken.refreshToken,
        rememberMe: authToken.rememberMe,
        Role: authToken.Role
      };
    }
    return null;
  }

  isLoggedIn(): boolean {
    return this.getAuthToken() != null;
  }

  public handleAsync(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, request).pipe(
      tap((response) => {
        this.setLoggedIn({
          accessToken: response.accessToken,
          refreshToken: response.refreshToken
        }, request.rememberMe ?? true)
      })
    );
  }

  setLoggedIn(lr: LoginResponse | null, rememberMe: boolean) {
    if (rememberMe) {
      if (lr == null) {
        window.localStorage.setItem('authToken', '');
      }
      else {
        window.localStorage.setItem('authToken', JSON.stringify(this.generateAuthTokenInfo(lr, rememberMe)));
      }
    }
    else {
      if (lr == null) {
        window.sessionStorage.setItem('authToken', '');
      }
      else {
        window.sessionStorage.setItem('authToken', JSON.stringify(this.generateAuthTokenInfo(lr, rememberMe)));
      }
    }
  }

  generateAuthTokenInfo(lr: LoginResponse, rememberMe: boolean): AuthTokenInfo {
    let decodedJwt: any = jwtDecode(lr.accessToken)
    return {
      nameid: Number.parseInt(decodedJwt.nameid),
      unique_name: decodedJwt.unique_name,
      email: decodedJwt.email,
      accessToken: lr.accessToken,
      refreshToken: lr.refreshToken,
      rememberMe: rememberMe,
      Role: decodedJwt.Role
    }
  }

  getAuthToken(): AuthTokenInfo | null {
    if (typeof window === 'undefined') {
      return null;
    }

    let tokenString = window.localStorage.getItem('authToken') ?? '';
    if (tokenString === '') {
      tokenString = window.sessionStorage.getItem('authToken') ?? '';
    }

    if (tokenString !== '') {
      let auth: AuthTokenInfo = JSON.parse(tokenString);

      if (this.refreshingToken) return null;
      if (this.isTokenExpired(auth!.accessToken)) {
        this.getNewJwt(auth).subscribe({
          error: (err: HttpErrorResponse) => {
            alert(err.error);
            this.setLoggedIn(null, auth.rememberMe);
            window.location.reload();
          }
        });
      }

      return auth;
    }

    return null;
  }

  getNewJwt(auth: AuthTokenInfo): Observable<LoginResponse> {
    if (this.refreshingToken) {
      return new Observable();
    }

    this.refreshingToken = true;

    console.log('Creating new access token.');
    return this.http.post<LoginResponse>(`${this.apiUrl}/refresh`, {
      refreshToken: auth.refreshToken
    }).pipe(
      tap(response => {
        this.setLoggedIn(response, auth.rememberMe);
        console.log('Token refreshed successfully');
        window.location.reload();
      }),
      finalize(() => {
        this.refreshingToken = false;
      })
    );
  }

  isTokenExpired(token: string): boolean {
    const decodedJwt: any = jwtDecode(token);
    return Date.now() > decodedJwt.exp! * 1000;
  }

  getUsername(): string | null {
    const authToken = this.getAuthToken();
    return authToken ? authToken.unique_name : null;
  }

  getUserRole(): string | null {
    const authToken = this.getAuthToken();
    return authToken ? authToken.Role : null;
  }
}
