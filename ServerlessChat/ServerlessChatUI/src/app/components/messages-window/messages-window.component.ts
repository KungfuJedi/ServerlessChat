import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Message } from 'src/app/models/message';
import { Store, select } from '@ngrx/store';
import * as fromRoot from 'src/app/selectors/message.selectors';
import { Observable } from 'rxjs';
import { AppState } from 'src/app/models/app.state';

@Component({
  selector: 'chat-messages-window',
  templateUrl: './messages-window.component.html',
  styleUrls: ['./messages-window.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MessagesWindowComponent implements OnInit {
  messages$: Observable<Message[]>;

  constructor(private store: Store<AppState>) { }

  ngOnInit() {
    this.messages$ = this.store.pipe(select(fromRoot.messages));
  }
}
