import { Message } from './message';

export interface MessageState {
    messages: Message[];
    messageContentToSend: string;
    isLoading: boolean;
}

export const defaultState: MessageState = {
    messages: [],
    messageContentToSend: '',
    isLoading: false
};