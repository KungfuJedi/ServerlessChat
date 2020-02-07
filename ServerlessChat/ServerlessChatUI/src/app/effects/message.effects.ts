import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { map } from 'rxjs/operators';
import { sendMessage, messageSent } from '../actions/message.actions';
import { WebsocketsService } from '../services/websockets.service';

@Injectable()
export class MessageEffects {
    constructor(private actions$: Actions, private websocketsService: WebsocketsService) {
    }

    sendMessage$ = createEffect(() => this.actions$
        .pipe(
            ofType(sendMessage),
            map(event => {
                this.websocketsService.sendMessage(event.content);
                return messageSent;
            })
        ))
}
