import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { CreatePostComponent } from "../../components/create-post/create-post.component";
import { PostItemComponent } from "../../components/post-item/post-item.component";
@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [MatTabsModule, MatButtonModule, MatIconModule, CreatePostComponent, PostItemComponent],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.scss'
})
export class ProfilePageComponent {

}
