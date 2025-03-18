import { Component } from '@angular/core';
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";

@Component({
  selector: 'request-item',
  standalone: true,
  imports: [UserAvatarComponent],
  templateUrl: './request-item.component.html',
  styleUrl: './request-item.component.scss'
})
export class RequestItemComponent {

}
