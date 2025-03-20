import { Component, computed, OnInit, signal } from '@angular/core';
import { PostItemComponent } from "../../components/post-item/post-item.component";
import { CreatePostComponent } from "../../components/create-post/create-post.component";
import { Router } from '@angular/router';
import { IdentityService } from '../../common/services/identity.service';
import { User } from '../../common/models/user.model';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [PostItemComponent, CreatePostComponent],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss'
})
export class HomePageComponent implements OnInit {
  currentUser = signal<User | null>(null);
  placeholderText = computed(() => `What's on your mind, ${this.currentUser()?.fullName}?`);

  constructor(private router: Router, private identitySvc: IdentityService) { }

  ngOnInit(): void {
    this.identitySvc.currentUser$.subscribe(user => {
      this.currentUser.set(user);
    })
  }
}
