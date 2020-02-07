import { createReducer, on, Action } from '@ngrx/store';
import { defaultState, MessageState } from '../models/message.state';
import { sendMessage, messageReceived, messageSent } from '../actions/message.actions';

const messageReducer = createReducer(
    defaultState,
    on(messageReceived, (state, { message }) => ({
        ...state,
        messages: state.messages.concat([message])
    })),
    on(sendMessage, (state, { content }) => ({
        ...state,
        messageContentToSend: content,
        isLoading: true
    })),
    on(messageSent, (state) => ({
        ...state,
        isLoading: false
    }))
);

export function reducer(state: MessageState | undefined, action: Action) {
    return messageReducer(state, action);
}