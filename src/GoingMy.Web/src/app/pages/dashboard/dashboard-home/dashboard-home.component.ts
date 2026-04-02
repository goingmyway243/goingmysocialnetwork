import { Component, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';

interface Post {
  id: string;
  title: string;
  content: string;
  author: string;
  createdAt: string;
  likes: number;
  comments: number;
}

@Component({
  selector: 'app-dashboard-home',
  imports: [CommonModule, CardModule, ButtonModule, SkeletonModule],
  templateUrl: './dashboard-home.component.html',
  styleUrl: './dashboard-home.component.css'
})
export class DashboardHomeComponent implements OnInit {
  posts = signal<Post[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    // this.loadPosts();
  }

  private loadPosts(): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<Post[]>('https://localhost:7002/api/posts').subscribe({
      next: (data) => {
        // Ensure data is always an array
        // this.posts.set(Array.isArray(data) ? data : []);
        this.setMockData();
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading posts:', err);
        this.error.set('Failed to load posts. Please try again later.');
        this.loading.set(false);
        // For demo purposes, set some mock data
        this.setMockData();
      }
    });
  }

  private setMockData(): void {
    const mockPosts: Post[] = [
      {
        id: '1',
        title: 'Welcome to GoingMySocial',
        content: 'This is a modern social network built with Angular and .NET. Stay connected with friends and share your moments!',
        author: 'System',
        createdAt: new Date().toISOString(),
        likes: 42,
        comments: 8
      },
      {
        id: '2',
        title: 'Getting Started',
        content: 'Explore the platform, connect with others, and share your thoughts. The journey begins here!',
        author: 'Admin',
        createdAt: new Date().toISOString(),
        likes: 28,
        comments: 5
      }
    ];
    this.posts.set(mockPosts);
  }

  likePost(post: Post): void {
    // Create a new array with the updated post to trigger signal update
    const updatedPosts = this.posts().map(p => 
      p.id === post.id ? { ...p, likes: p.likes + 1 } : p
    );
    this.posts.set(updatedPosts);
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    return date.toLocaleDateString();
  }
}
