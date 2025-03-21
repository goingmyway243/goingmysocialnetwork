import { Component, OnInit } from '@angular/core';
import { User } from '../../common/models/user.model';
import { AuthService } from '../../common/services/auth.service';

@Component({
  selector: 'app-common',
  standalone: true,
  imports: [],
  template: ''
})
export class AppCommonComponent implements OnInit {
  currentUser!: User | null;

  constructor(public authSvc: AuthService) {
  }

  ngOnInit(): void {
    this.authSvc.currentUser$.subscribe(user => {
      this.currentUser = user;
    })
  }
}
