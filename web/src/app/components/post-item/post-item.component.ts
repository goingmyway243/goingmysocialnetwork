import { Component } from '@angular/core';
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { MatIconModule } from "@angular/material/icon";
@Component({
  selector: 'post-item',
  standalone: true,
  imports: [UserAvatarComponent, MatIconModule],
  templateUrl: './post-item.component.html',
  styleUrl: './post-item.component.scss'
})
export class PostItemComponent {

}
