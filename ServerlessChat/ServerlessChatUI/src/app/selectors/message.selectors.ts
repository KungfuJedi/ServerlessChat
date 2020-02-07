import { createSelector } from '@ngrx/store'
import { AppState } from '../models/app.state';

export const appState = (state: AppState) => state;

export const messages = createSelector(
    appState,
    (state: AppState) => state.messages.messages
)