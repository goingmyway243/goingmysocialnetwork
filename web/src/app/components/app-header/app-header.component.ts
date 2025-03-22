import { Component } from '@angular/core';
import { MatMenuModule } from '@angular/material/menu';
import { SearchBarComponent } from "../search-bar/search-bar.component";
import { CreatePostComponent } from "../create-post/create-post.component";
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { Router } from '@angular/router';
import { AppCommonComponent } from '../app-common/app-common.component';
import { AuthService } from '../../common/services/auth.service';

@Component({
  selector: 'app-header',
  templateUrl: './app-header.component.html',
  styleUrl: './app-header.component.scss',
  standalone: true,
  imports: [MatMenuModule, SearchBarComponent, CreatePostComponent, UserAvatarComponent]
})
export class AppHeaderComponent extends AppCommonComponent {
  constructor(public router: Router, authSvc: AuthService) {
    super(authSvc);
  }

  public logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }

  public navigateToProfile() {
    this.router.navigate(['/profile']);
  }
}
