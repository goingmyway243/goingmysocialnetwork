import { Component, computed, effect, OnDestroy, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { CreatePostComponent } from "../../components/create-post/create-post.component";
import { PostItemComponent } from "../../components/post-item/post-item.component";
import { AuthService } from '../../common/services/auth.service';
import { User } from '../../common/models/user.model';
import { Post } from '../../common/models/post.model';
import { Router } from '@angular/router';
import { PostApiService } from '../../common/services/post-api.service';
import { environment } from '../../../environments/environment';
import { UserApiService } from '../../common/services/user-api.service';
import { MatDialog } from '@angular/material/dialog';
import { NotificationDialogComponent } from '../../dialogs/notification-dialog/notification-dialog.component';
import { Util } from '../../common/helpers/util';


@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [MatTabsModule, MatButtonModule, MatIconModule, CreatePostComponent, PostItemComponent],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.scss'
})
export class ProfilePageComponent implements OnInit, OnDestroy {
  currentUser = signal<User | null>(null);
  placeholderText = computed(() =>
    this.currentUser()
      ? `What's on your mind, ${this.currentUser()?.fullName}?`
      : 'What\'s on your mind?'
  );

  avatar = signal(environment.defaultAvatar);
  uploadedAvatar = signal('');

  postItems = signal<Post[]>([]);

  postLoaded: boolean = false;
  uploadedAvatarFile?: File;

  constructor(
    private router: Router,
    private authSvc: AuthService,
    private userApiSvc: UserApiService,
    private postApiSvc: PostApiService,
    private dialog: MatDialog
  ) {
    effect(() => {
      if (this.currentUser() && !this.postLoaded) {
        this.postApiSvc.searchPosts({
          pagedRequest: {
            cursorTimestamp: Util.getUtcNow(),
            pageSize: 10
          },
          currentUserId: this.currentUser()!.id,
          ownerId: this.currentUser()!.id
        }).subscribe(result => {
          this.postItems.set(result.items);
          this.postLoaded = true;
        });
      }
    })
  }

  ngOnDestroy(): void {
    this.revokeUploadedAvatar();
  }

  ngOnInit(): void {
    this.authSvc.currentUser$.subscribe(user => {
      this.currentUser.set(user);
      this.avatar.set(this.currentUser()?.profilePicture ?? environment.defaultAvatar);
    });
  }

  onFileSelected(evt: any): void {
    const file = evt.target.files[0];
    if (file) {
      this.uploadedAvatarFile = file;
      this.revokeUploadedAvatar();
      this.uploadedAvatar.set(URL.createObjectURL(file));
    }
  }

  uploadAvatar(): void {
    this.userApiSvc.updateUserAvatar(this.currentUser()!.id, this.uploadedAvatarFile!)
      .subscribe(result => {
        if (result.url) {
          const cacheBustedUrl = `${result.url}?t=${new Date().getTime()}`;
          this.currentUser.update(user => {
            if (user) {
              user.profilePicture = cacheBustedUrl;
            }

            return user;
          });

          this.revokeUploadedAvatar();
          this.avatar.set(cacheBustedUrl);

          this.dialog.open(NotificationDialogComponent, {
            data: {
              message: 'Avatar uploaded! Reload the page to see the changes fully.',
              durationMs: 3000
            },
            position: {
              top: '84px',
              right: '12px'
            },
            maxWidth: '320px',
            panelClass: 'custom-panel-dialog',
            hasBackdrop: false
          });
        }
      });
  }

  revokeUploadedAvatar(): void {
    if (this.uploadedAvatar()) {
      URL.revokeObjectURL(this.uploadedAvatar());
      this.uploadedAvatar.set('');
    }
  }
}
