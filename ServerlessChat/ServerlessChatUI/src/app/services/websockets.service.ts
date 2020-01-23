import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { AppStateService } from './app-state.service';
import { MessagesService } from './messages.service';

@Injectable({
  providedIn: 'root'
})
export class WebsocketsService {
  private socket: WebSocket;

  constructor(private appStateService: AppStateService, private messageServices: MessagesService) { }

  connect() {
    this.socket = new WebSocket(environment.websocketUrl, [this.appStateService.authToken]);
    this.socket.onmessage = (message) => {
      const newMessage = JSON.parse(message.data);
      this.messageServices.onNewMessage(newMessage);
    };
  }
}
