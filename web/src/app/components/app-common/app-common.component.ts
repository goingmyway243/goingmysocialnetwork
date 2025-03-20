import { Component, OnInit } from '@angular/core';
import { IdentityService } from '../../common/services/identity.service';
import { User } from '../../common/models/user.model';

@Component({
  selector: 'app-common',
  standalone: true,
  imports: [],
  template: ''
})
export class AppCommonComponent implements OnInit {
  currentUser!: User | null;

  constructor(public identitySvc: IdentityService) {
  }

  ngOnInit(): void {
    this.identitySvc.currentUser$.subscribe(user => {
      this.currentUser = user;
    })
  }
}
