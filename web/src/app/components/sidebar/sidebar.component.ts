import { Component } from '@angular/core';
import { CreatePostComponent } from "../create-post/create-post.component";
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { Router } from '@angular/router';

@Component({
    selector: 'app-sidebar',
    templateUrl: './sidebar.component.html',
    styleUrl: './sidebar.component.scss',
    standalone: true,
    imports: [CreatePostComponent, UserAvatarComponent]
})
export class SidebarComponent {
    constructor(private router: Router) { }

    public navigateTo(path: string): void {
        this.router.navigate([path]);
    }
}
