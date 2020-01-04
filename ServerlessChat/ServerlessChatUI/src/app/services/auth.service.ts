import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private http: HttpClient) { }

  signIn(username: string): Observable<SignInResponse> {
    return this.http.post<SignInResponse>(`${environment.baseUrl}/sign-in`, {username});
  }
}

export interface SignInResponse {
  AuthToken: string;
}
