import { Component, OnInit, ChangeDetectionStrategy, Input } from '@angular/core';
import { Message } from 'src/app/models/message';

@Component({
  selector: 'chat-message',
  templateUrl: './message.component.html',
  styleUrls: ['./message.component.scss']
})
export class MessageComponent implements OnInit {
  @Input() message: Message;

  constructor() { }

  ngOnInit() {
  }

}
