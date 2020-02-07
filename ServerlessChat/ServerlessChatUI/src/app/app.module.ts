import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {MatToolbarModule} from '@angular/material/toolbar';
import {MatInputModule} from '@angular/material/input';
import {MatDividerModule} from '@angular/material/divider';
import { SendMessageComponent } from './components/send-message/send-message.component';
import {MatButtonModule} from '@angular/material/button';
import { MessagesWindowComponent } from './components/messages-window/messages-window.component';
import { MessageComponent } from './components/message/message.component';
import { SignInComponent } from './components/sign-in/sign-in.component';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { StoreModule } from '@ngrx/store';
import * as fromMessages from './reducers/message.reducer';
import { environment } from 'src/environments/environment';
import { StoreDevtoolsModule } from '@ngrx/store-devtools';
import { EffectsModule } from '@ngrx/effects';
import { MessageEffects } from './effects/message.effects';

@NgModule({
  declarations: [
    AppComponent,
    SendMessageComponent,
    MessagesWindowComponent,
    MessageComponent,
    SignInComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MatToolbarModule,
    MatInputModule,
    MatDividerModule,
    MatButtonModule,
    FormsModule,
    HttpClientModule,
    StoreModule.forRoot({ messages: fromMessages.reducer }),
    StoreDevtoolsModule.instrument(
      environment.production
      ? {}
      : {
        maxAge: 25
        }),
    EffectsModule.forRoot([MessageEffects])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
