import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AppStateService {
  authToken: string;

  private isSignedInSubject: BehaviorSubject<boolean> = new BehaviorSubject(false);

  get isSignedIn$(): Observable<boolean> {
    return this.isSignedInSubject.asObservable();
  }

  constructor() { }

  applyAuthToken(authToken: string): void {
    this.authToken = authToken;
    this.isSignedInSubject.next(true);
  }
}
