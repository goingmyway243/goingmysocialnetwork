import { Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  imports: [],
  templateUrl: './empty-state.component.html',
  styleUrl: './empty-state.component.css'
})
export class EmptyStateComponent {
  icon = input<string>('pi-inbox');
  title = input<string>('Nothing here yet');
  subtitle = input<string>('');
}
