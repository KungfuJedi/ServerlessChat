import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpResponseBase, HttpHeaders } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { Message } from '../models/message';
import { AppStateService } from './app-state.service';

@Injectable({
  providedIn: 'root'
})
export class MessagesService {

  constructor(private http: HttpClient, private appStateService: AppStateService) { }

  getMessages(): Observable<GetMessagesResponse> {
    return this.http.get<GetMessagesResponse>(`${environment.baseUrl}/messages`);
  }

  sendMessage(content: string): Observable<HttpResponseBase> {
    console.log(content);
    console.log(this.appStateService.authToken);
    return this.http.post<HttpResponseBase>(`${environment.baseUrl}/message`, {Content: content},
      {headers: new HttpHeaders().set('Authorization', this.appStateService.authToken)});
  }
}

export interface GetMessagesResponse {
  Messages: Message[];
}
