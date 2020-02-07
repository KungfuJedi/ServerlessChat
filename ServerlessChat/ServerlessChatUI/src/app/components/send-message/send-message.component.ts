import { Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { AppState } from 'src/app/models/app.state';
import { sendMessage } from 'src/app/actions/message.actions';

@Component({
  selector: 'chat-send-message',
  templateUrl: './send-message.component.html',
  styleUrls: ['./send-message.component.scss']
})
export class SendMessageComponent implements OnInit {
  message: string;

  constructor(private store: Store<AppState>) { }

  ngOnInit() {
  }

  sendMessage(): void {
    if (this.message.length === 0) {
      return;
    }

    this.store.dispatch(sendMessage({content: this.message}));
    this.message = '';
  }
}
