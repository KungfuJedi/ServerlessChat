import { Component, OnInit } from '@angular/core';
import { Message } from 'src/app/models/message';
import { MessagesService } from 'src/app/services/messages.service';

@Component({
  selector: 'chat-messages-window',
  templateUrl: './messages-window.component.html',
  styleUrls: ['./messages-window.component.scss']
})
export class MessagesWindowComponent implements OnInit {
  messages: Message[];

  constructor(private messagesService: MessagesService) { }

  ngOnInit() {
    this.messagesService.getMessages()
      .subscribe(res => this.messages = res.Messages);
  }
}
