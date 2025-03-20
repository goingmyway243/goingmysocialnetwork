import { NgIf } from '@angular/common';
import { Component, Input } from '@angular/core';
import { User } from '../../common/models/user.model';

@Component({
  selector: 'user-avatar',
  standalone: true,
  imports: [NgIf],
  templateUrl: './user-avatar.component.html',
  styleUrl: './user-avatar.component.scss'
})
export class UserAvatarComponent {
  @Input({ required: true }) user!: User | null;
  @Input() showInfo: boolean = true;
}
