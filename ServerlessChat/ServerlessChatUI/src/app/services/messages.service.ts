import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { HttpClient, HttpResponseBase, HttpHeaders } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { Message } from '../models/message';
import { AppStateService } from './app-state.service';

@Injectable({
  providedIn: 'root'
})
export class MessagesService {
  private newMessagesSubject: Subject<Message> = new Subject<Message>();

  constructor(private http: HttpClient, private appStateService: AppStateService) { }

  getMessages(): Observable<GetMessagesResponse> {
    return this.http.get<GetMessagesResponse>(`${environment.baseUrl}/messages`);
  }

  sendMessage(content: string): Observable<HttpResponseBase> {
    return this.http.post<HttpResponseBase>(`${environment.baseUrl}/message`, {Content: content},
      {headers: new HttpHeaders().set('Authorization', this.appStateService.authToken)});
  }

  onNewMessage(message: Message): void {
    this.newMessagesSubject.next(message);
  }

  newMessages$(): Observable<Message> {
    return this.newMessagesSubject.asObservable();
  }
}

export interface GetMessagesResponse {
  Messages: Message[];
}
