import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { RequestBoxComponent } from "../../components/request-box/request-box.component";
import { MessageBoxComponent } from "../../components/message-box/message-box.component";
import { SidebarComponent } from "../../components/app-sidebar/app-sidebar.component";
import { AppHeaderComponent } from "../../components/app-header/app-header.component";
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';
import { Subscription } from 'rxjs';
import { IdentityService } from '../../common/services/identity.service';

@Component({
  selector: 'dashboard-page',
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RequestBoxComponent,
    MessageBoxComponent,
    SidebarComponent,
    AppHeaderComponent
  ]
})
export class DashboardPageComponent implements OnInit, OnDestroy {
  showOptionalSidebar = signal(false);

  private _routerSubscription: Subscription | undefined;

  constructor
    (
      private router: Router,
      private identityService: IdentityService
    ) { }

  ngOnInit(): void {
    this.fetchUserInfoIfNeeded();

    // Initial check
    this.showOptionalSidebar.set(this.router.url === '/' || this.router.url === '/home');

    // Subscribe to router events
    this.subscribeRouterEvents();
  }

  private fetchUserInfoIfNeeded() {
    const isAuthenticated = this.identityService.isAuthenticated();
    const currentUser = this.identityService.getCurrentUser();
    if (isAuthenticated && !currentUser) {
      this.identityService.getUserInfoAsync().subscribe();
    }
  }

  private subscribeRouterEvents() {
    this._routerSubscription = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.showOptionalSidebar.set(event.url === '/' || event.url === '/home');
      }
    });
  }

  ngOnDestroy(): void {
    if (this._routerSubscription) {
      this._routerSubscription.unsubscribe();
    }
  }
}
