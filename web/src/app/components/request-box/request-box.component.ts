import { Component } from '@angular/core';
import { RequestItemComponent } from "../request-item/request-item.component";

@Component({
  selector: 'request-box',
  standalone: true,
  imports: [RequestItemComponent],
  templateUrl: './request-box.component.html',
  styleUrl: './request-box.component.scss'
})
export class RequestBoxComponent {

}
