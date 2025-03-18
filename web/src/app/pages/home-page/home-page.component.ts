import { Component } from '@angular/core';
import { PostItemComponent } from "../../components/post-item/post-item.component";
import { CreatePostComponent } from "../../components/create-post/create-post.component";

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [PostItemComponent, CreatePostComponent],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss'
})
export class HomePageComponent {
  public placeholderText: string = "What's on your mind, Light?";

}
