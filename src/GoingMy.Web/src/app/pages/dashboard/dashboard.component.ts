import { Component, inject } from '@angular/core';
import { Router, RouterOutlet, NavigationStart } from '@angular/router';
import { DashboardHeaderComponent } from '../../components/dashboard-header/dashboard-header.component';
import { DashboardSidebarComponent } from '../../components/dashboard-sidebar/dashboard-sidebar.component';
import { MiniChatComponent } from '../../components/mini-chat/mini-chat.component';
import { LayoutService } from '../../services/layout.service';

@Component({
  selector: 'app-dashboard',
  imports: [RouterOutlet, DashboardHeaderComponent, DashboardSidebarComponent, MiniChatComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent {
  readonly layout = inject(LayoutService);

  constructor() {
    // Reset sidebar visibility on every navigation so only the current page controls it
    inject(Router).events.subscribe(e => {
      if (e instanceof NavigationStart) this.layout.hideSidebar.set(false);
    });
  }
}
