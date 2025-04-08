import { NgIf } from '@angular/common';
import { Component, computed, input, Input } from '@angular/core';
import { User } from '../../common/models/user.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'user-avatar',
  standalone: true,
  imports: [NgIf],
  templateUrl: './user-avatar.component.html',
  styleUrl: './user-avatar.component.scss'
})
export class UserAvatarComponent {
  @Input() showInfo: boolean = true;
  @Input() description: string = '';

  user = input.required<User | null | undefined>();
  avatar = computed(() => {
    if (this.user()) {
      return this.user()?.profilePicture ?? environment.defaultAvatar;
    }

    return environment.defaultAvatar;
  });
}
