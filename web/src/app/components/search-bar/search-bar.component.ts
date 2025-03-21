import { Component, Input, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'search-bar',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './search-bar.component.html',
  styleUrl: './search-bar.component.scss'
})
export class SearchBarComponent implements OnInit {
  @Input() placeholder: string = '';

  searchText = '';

  constructor(private router: Router, private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(query => {
      this.searchText = query.get('q') ?? '';
    })
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.performSearch();
    }
  }

  clearSearchText(): void {
    this.searchText = '';
  }

  performSearch(): void {
    if (this.searchText) {
      this.router.navigate(['/explore'], { queryParams: { q: this.searchText } });
    }
  }
}
