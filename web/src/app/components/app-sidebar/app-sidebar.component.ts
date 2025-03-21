import { Component } from '@angular/core';
import { CreatePostComponent } from "../create-post/create-post.component";
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { Router } from '@angular/router';
import { AppCommonComponent } from '../app-common/app-common.component';
import { AuthService } from '../../common/services/auth.service';

@Component({
    selector: 'app-sidebar',
    templateUrl: './app-sidebar.component.html',
    styleUrl: './app-sidebar.component.scss',
    standalone: true,
    imports: [CreatePostComponent, UserAvatarComponent]
})
export class SidebarComponent extends AppCommonComponent {
    constructor(public router: Router, authSvc: AuthService) {
        super(authSvc);
      }

    navigateTo(path: string): void {
        this.router.navigate([path]);
    }
}
