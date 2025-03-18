import { Component, ViewEncapsulation } from '@angular/core';
import { SearchBarComponent } from "../search-bar/search-bar.component";
import { AppTabstripComponent } from "../app-tabstrip/app-tabstrip.component";
import { AppTabstripTabComponent } from "../app-tabstrip/app-tabstrip-tab.component";
import { MessageItemComponent } from "../message-item/message-item.component";

@Component({
  selector: 'message-box',
  standalone: true,
  imports: [SearchBarComponent, AppTabstripComponent, AppTabstripTabComponent, MessageItemComponent],
  templateUrl: './message-box.component.html',
  styleUrl: './message-box.component.scss',
  encapsulation: ViewEncapsulation.None
})
export class MessageBoxComponent {

}
