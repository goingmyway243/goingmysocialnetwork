import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChartModule } from 'primeng/chart';
import { AdminApiService, UserStats, PostStats } from '../../../services/admin-api.service';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-admin-dashboard',
  imports: [CommonModule, ChartModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  private readonly _adminApi = inject(AdminApiService);

  // ── State ─────────────────────────────────────────────────────
  readonly isLoading = signal(true);
  readonly userStats = signal<UserStats | null>(null);
  readonly postStats = signal<PostStats | null>(null);
  readonly error = signal<string | null>(null);

  // ── Charts ────────────────────────────────────────────────────
  readonly registrationChartData = computed(() => {
    const stats = this.userStats();
    if (!stats) return null;
    return {
      labels: stats.registrationsLast30Days.map(d => d.date),
      datasets: [{
        label: 'New Users',
        data: stats.registrationsLast30Days.map(d => d.count),
        fill: true,
        borderColor: '#60a5fa',
        backgroundColor: 'rgba(96, 165, 250, 0.15)',
        tension: 0.4,
        pointRadius: 3,
        pointBackgroundColor: '#60a5fa'
      }]
    };
  });

  readonly chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false }
    },
    scales: {
      x: {
        ticks: { color: 'rgba(255,255,255,0.5)', maxTicksLimit: 10, font: { size: 11 } },
        grid: { color: 'rgba(255,255,255,0.05)' }
      },
      y: {
        ticks: { color: 'rgba(255,255,255,0.5)', font: { size: 11 } },
        grid: { color: 'rgba(255,255,255,0.05)' },
        beginAtZero: true
      }
    }
  };

  // ── Lifecycle ─────────────────────────────────────────────────
  ngOnInit(): void {
    forkJoin({
      users: this._adminApi.getUserStats(),
      posts: this._adminApi.getPostStats()
    }).subscribe({
      next: ({ users, posts }) => {
        this.userStats.set(users);
        this.postStats.set(posts);
        this.isLoading.set(false);
      },
      error: () => {
        this.error.set('Failed to load statistics. Please try again.');
        this.isLoading.set(false);
      }
    });
  }
}
