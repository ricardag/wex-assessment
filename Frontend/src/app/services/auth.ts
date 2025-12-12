import {Injectable, signal} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Router} from '@angular/router';
import {firstValueFrom, Observable, tap} from 'rxjs';
import {environment} from '../../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';

  public isAuthenticated = signal<boolean>(this.isLoggedIn());

  private tokenCheckIntervalId: any = null;

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.startTokenExpirationCheck();
  }

  private startTokenExpirationCheck(): void {
    if (this.tokenCheckIntervalId !== null) {
      return;
    }

    this.tokenCheckIntervalId = setInterval(() => {
      if (!this.isAuthenticated()) {
        return;
      }
      const token = this.getToken();
      if (!token) {
        return;
      }

      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const expiry = payload.exp;
        const currentTime = Math.floor(new Date().getTime() / 1000);
        const timeLeft = expiry - currentTime;

        if (timeLeft > 0 && timeLeft < 60) {
          this.refreshToken();
        }
      } catch {
        // If token is invalid, do nothing here
      }
    }, 10000); // Check each 10 s
  }

  private stopTokenExpirationCheck(): void {
    if (this.tokenCheckIntervalId !== null) {
      clearInterval(this.tokenCheckIntervalId);
      this.tokenCheckIntervalId = null;
    }
  }

  public async login(credentials: LoginCredentials): Promise<boolean> {
    try {
      const response = await firstValueFrom(
        this.http.post<AuthResponse>(`${environment.apiUrl}/auth`, credentials)
      );
      this.setSession(response);
      return true;
    } catch (error) {
      return false;
    }
  }

  public logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this.isAuthenticated.set(false);
    this.stopTokenExpirationCheck();
  }

  public isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token)
      return false;

    // Verifica se o token expirou
    return !this.isTokenExpired(token);
  }

  public getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  public async refreshToken(): Promise<boolean> {
    const storedToken = this.getToken();

    // Se nao temos token ou esta expirado, força logout e redirect para a home
    if (!storedToken || this.isTokenExpired(storedToken)) {
      console.warn('Refresh token aborted: stored token is missing or expired. Logging out.');
      this.logout();
      await this.router.navigateByUrl('/');
      return false;
    }

    console.log('Will refresh token');
    try {
      const response = await firstValueFrom(
        this.http.put<AuthResponse>(`${environment.apiUrl}/auth`, null)
      );
      this.setSession(response);
      return true;
    } catch (error) {
      console.error('Error on refresh token', error);
      // If refresh fails for any reason, logout to clear state and redirect.
      this.logout();
      await this.router.navigateByUrl('/');
      return false;
    }

  }

  private setSession(authResponse: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, authResponse.token);
    this.isAuthenticated.set(true);
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp;
      return Math.floor(new Date().getTime() / 1000) >= expiry;
    } catch {
      return true;
    }
  }

}
