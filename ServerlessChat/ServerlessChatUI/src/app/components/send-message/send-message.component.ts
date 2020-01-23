import { Component, OnInit } from '@angular/core';
import { MessagesService } from 'src/app/services/messages.service';

@Component({
  selector: 'chat-send-message',
  templateUrl: './send-message.component.html',
  styleUrls: ['./send-message.component.scss']
})
export class SendMessageComponent implements OnInit {
  message: string;

  constructor(private messagesService: MessagesService) { }

  ngOnInit() {
  }

  sendMessage(): void {
    this.messagesService.sendMessage(this.message)
      .subscribe(_ => this.message = '');
  }
}
