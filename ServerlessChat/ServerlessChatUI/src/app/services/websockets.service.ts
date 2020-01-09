import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { AppStateService } from './app-state.service';

@Injectable({
  providedIn: 'root'
})
export class WebsocketsService {
  private socket: WebSocket;

  constructor(private appStateService: AppStateService) { }

  connect() {
    this.socket = new WebSocket(environment.websocketUrl, [this.appStateService.authToken]);
  }
}
