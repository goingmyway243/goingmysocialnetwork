import { Component, OnInit, OnDestroy } from '@angular/core';
import { RequestBoxComponent } from "../../components/request-box/request-box.component";
import { MessageBoxComponent } from "../../components/message-box/message-box.component";
import { SidebarComponent } from "../../components/sidebar/sidebar.component";
import { AppHeaderComponent } from "../../components/app-header/app-header.component";
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';
import { Subscription } from 'rxjs';

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
  public placeholderText: string = "What's on your mind, Light?";
  public showOptionalSidebar: boolean = false;
  private routerSubscription: Subscription | undefined;

  constructor(private router: Router) {}

  ngOnInit(): void {
    // Initial check
    this.showOptionalSidebar = this.router.url === '/' || this.router.url === '/home';

    // Subscribe to router events
    this.routerSubscription = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.showOptionalSidebar = event.url === '/' || event.url === '/home';
      }
    });
  }

  ngOnDestroy(): void {
    if (this.routerSubscription) {
      this.routerSubscription.unsubscribe();
    }
  }
}
