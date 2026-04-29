import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LayoutService {
  /** When true, the dashboard sidebar is hidden. Reset to false on navigation. */
  readonly hideSidebar = signal(false);
}
