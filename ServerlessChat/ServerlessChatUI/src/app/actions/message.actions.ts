import { createAction, props } from '@ngrx/store';
import { Message } from '../models/message';

export const sendMessage = createAction('[Messages] SendMessage', props<{content: string}>());
export const messageReceived = createAction('[Messages] MessageReceived', props<{message: Message}>());
export const messageSent = createAction('[Messages] MessageSent');
