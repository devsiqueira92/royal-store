import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RegisterRequest } from '../interfaces/register-request.interface';
import { LoginRequest } from '../interfaces/login-request.interface';
import { TokenResponse } from '../interfaces/token-response.interface';
import { ForgotPasswordRequest } from '../interfaces/forgot-password-request.interface';
import { ResendConfirmationEmailRequest } from '../interfaces/resend-confirmation-email-request.interface';
import { ResetPasswordRequest } from '../interfaces/reset-password-request.interface';
import { MessageResponse } from '../interfaces/message-response.interface';
import { User } from '../interfaces/user.interface';
import { BYPASS_GLOBAL_ERROR } from '../interceptor/bypass-error.context';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/auth`;

  getUserProfile(): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/session`);
  }

  refreshSession(): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/refresh`, {});
  }

  logout(): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/logout`, {}, {
      context: new HttpContext().set(BYPASS_GLOBAL_ERROR, true),
    });
  }

  register(request: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/register`, request, {
      context: new HttpContext().set(BYPASS_GLOBAL_ERROR, true),
    });
  }

  login(request: LoginRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.apiUrl}/login`, request, {
      context: new HttpContext().set(BYPASS_GLOBAL_ERROR, true),
    });
  }

  verifyEmail(userId: string, token: string): Observable<MessageResponse> {
    return this.http.get<MessageResponse>(`${this.apiUrl}/verify-email`, {
      params: { userId, token },
      context: new HttpContext().set(BYPASS_GLOBAL_ERROR, true),
    });
  }

  resendConfirmationEmail(
    request: ResendConfirmationEmailRequest,
  ): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(
      `${this.apiUrl}/resend-confirmation-email`,
      request,
    );
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(
      `${this.apiUrl}/forgot-password`,
      request,
    );
  }

  resetPassword(request: ResetPasswordRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(
      `${this.apiUrl}/reset-password`,
      request,
    );
  }
}
