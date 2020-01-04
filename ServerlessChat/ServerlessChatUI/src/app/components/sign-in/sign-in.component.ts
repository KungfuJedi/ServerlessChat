import { Component, OnInit } from '@angular/core';
import { AuthService } from 'src/app/services/auth.service';
import { AppStateService } from 'src/app/services/app-state.service';

@Component({
  selector: 'chat-sign-in',
  templateUrl: './sign-in.component.html',
  styleUrls: ['./sign-in.component.scss']
})
export class SignInComponent implements OnInit {
  public username: string;

  constructor(private authService: AuthService, private appStateService: AppStateService) { }

  ngOnInit() {
  }

  signIn(): void {
    this.authService.signIn(this.username)
      .subscribe(res => this.appStateService.applyAuthToken(res.AuthToken));
  }
}
