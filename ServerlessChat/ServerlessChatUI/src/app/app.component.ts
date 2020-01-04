import { Component } from '@angular/core';
import { AppStateService } from './services/app-state.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'chat-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'ServerlessChatUI';
  isSignedIn$: Observable<boolean> = this.appStateService.isSignedIn$;

  constructor(private appStateService: AppStateService) {

  }
}
