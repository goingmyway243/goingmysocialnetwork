import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DashboardHeaderComponent } from '../../components/dashboard-header/dashboard-header.component';
import { DashboardSidebarComponent } from '../../components/dashboard-sidebar/dashboard-sidebar.component';
import { MiniChatComponent } from '../../components/mini-chat/mini-chat.component';

@Component({
  selector: 'app-dashboard',
  imports: [RouterOutlet, DashboardHeaderComponent, DashboardSidebarComponent, MiniChatComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent {}
