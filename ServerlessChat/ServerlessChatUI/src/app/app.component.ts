import { Component, OnInit } from '@angular/core';
import { AppStateService } from './services/app-state.service';
import { Observable } from 'rxjs';
import { first, filter } from 'rxjs/operators';
import { WebsocketsService } from './services/websockets.service';

@Component({
  selector: 'chat-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'ServerlessChatUI';
  isSignedIn$: Observable<boolean> = this.appStateService.isSignedIn$;

  constructor(private appStateService: AppStateService, private websocketsService: WebsocketsService) {

  }

  ngOnInit() {
    this.appStateService.isSignedIn$
      .pipe(
        filter(isSignedIn => isSignedIn),
        first()
      )
      .subscribe(_ => this.websocketsService.connect());
  }
}
