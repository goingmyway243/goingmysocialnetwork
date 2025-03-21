import { Component, Input } from '@angular/core';
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { User } from '../../common/models/user.model';

@Component({
  selector: 'user-item',
  standalone: true,
  imports: [UserAvatarComponent, MatButtonModule, MatIconModule],
  templateUrl: './user-item.component.html',
  styleUrl: './user-item.component.scss'
})
export class UserItemComponent {
  @Input() userData!: User;
}
