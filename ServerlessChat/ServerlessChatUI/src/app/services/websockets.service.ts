import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { AppStateService } from './app-state.service';
import { Store } from '@ngrx/store';
import { MessageState } from '../models/message.state';
import { messageReceived } from '../actions/message.actions';

@Injectable({
  providedIn: 'root'
})
export class WebsocketsService {
  private socket: WebSocket;

  private readonly registerAction = 'register';
  private readonly sendMessageAction = 'sendMessage';

  constructor(private appStateService: AppStateService, private store: Store<MessageState>) { }

  connect() {
    this.socket = new WebSocket(environment.websocketUrl);

    this.socket.onopen = () => {
      this.socket.send(JSON.stringify({
        action: this.registerAction,
        authToken: this.appStateService.authToken
      }));
    }

    this.socket.onmessage = (message) => {
      const newMessage = JSON.parse(message.data);
      this.store.dispatch(messageReceived({message: newMessage}));
    };
  }

  public sendMessage(content: string) {
    this.socket.send(JSON.stringify({
      action: this.sendMessageAction,
      authToken: this.appStateService.authToken,
      content
    }));
  }
}
