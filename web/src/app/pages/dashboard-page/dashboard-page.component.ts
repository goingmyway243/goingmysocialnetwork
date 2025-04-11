import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { RequestBoxComponent } from "../../components/request-box/request-box.component";
import { MessageBoxComponent } from "../../components/message-box/message-box.component";
import { SidebarComponent } from "../../components/app-sidebar/app-sidebar.component";
import { AppHeaderComponent } from "../../components/app-header/app-header.component";
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';
import { Subscription } from 'rxjs';
import { IdentityApiService } from '../../common/services/identity-api.service';
import { ThemeManagerService } from '../../common/services/theme-manager.service';

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
      private identityApiService: IdentityApiService,
      private themeManagerSvc: ThemeManagerService
    ) { }

  ngOnInit(): void {
    this.identityApiService.fetchUserInfoIfNeeded().subscribe();

    // Initial check
    this.showOptionalSidebar.set(this.router.url === '/' || this.router.url === '/home');

    // Subscribe to router events
    this.subscribeRouterEvents();

    // Load theme customization
    this.themeManagerSvc.laodThemeCustomization();
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
