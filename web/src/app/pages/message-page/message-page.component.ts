import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { UserAvatarComponent } from "../../components/user-avatar/user-avatar.component";
import { MessageItemComponent } from "../../components/message-item/message-item.component";

@Component({
  selector: 'app-message-page',
  standalone: true,
  imports: [MatIconModule, UserAvatarComponent, MessageItemComponent],
  templateUrl: './message-page.component.html',
  styleUrl: './message-page.component.scss'
})
export class MessagePageComponent {

}
