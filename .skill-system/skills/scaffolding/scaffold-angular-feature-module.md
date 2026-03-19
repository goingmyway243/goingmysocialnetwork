# SKILL: scaffold-angular-feature-module

## Purpose
Generate a new Angular feature module with components, services, routing, and structure following best practices.

## Use When
- Creating new feature area (User, Post, Notification modules)
- Adding major new functionality
- Organizing related features

## Required Inputs
- Feature name (e.g., user, post, notification)
- Components needed (list, detail, create/edit)
- Routes and navigation

## Expected Output
- Feature module with proper structure
- Lazy-loaded route configuration
- Smart and presentational components
- Feature service
- Test files configured

## Execution Approach

### Step 1: Create Feature Module Structure
```bash
# Generate feature folder
mkdir -p src/app/features/{feature}/{components,services,models,directives}

# Create module file
ng generate module features/{feature}/{feature} \
  --routing \
  --flat=false
```

### Step 2: Create Feature Structure
```
src/app/features/{feature}/
├── components/
│   ├── {feature}-list/
│   │   ├── {feature}-list.component.ts
│   │   ├── {feature}-list.component.html
│   │   ├── {feature}-list.component.scss
│   │   └── {feature}-list.component.spec.ts
│   ├── {feature}-detail/
│   │   └── ...
│   └── {feature}-form/
│       └── ...
├── services/
│   ├── {feature}.service.ts
│   └── {feature}.service.spec.ts
├── models/
│   └── {feature}.model.ts
├── {feature}-routing.module.ts
├── {feature}.module.ts
└── README.md
```

### Step 3: Create Components

**List Component (Smart) - Using Signals:**
```typescript
import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { {Feature}Service } from '../../services/{feature}.service';

@Component({
  selector: 'app-{feature}-list',
  templateUrl: './{feature}-list.component.html',
  styleUrls: ['./{feature}-list.component.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class {Feature}ListComponent {
  private {feature}Service = inject({Feature}Service);
  
  items = signal<{Feature}[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  
  ngOnInit() {
    this.loadItems();
  }
  
  private loadItems() {
    this.loading.set(true);
    this.{feature}Service.getAll().subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load items');
        this.loading.set(false);
      }
    });
  }
  
  onDelete(id: number) {
    this.{feature}Service.delete(id).subscribe(() => {
      this.items.update(items => items.filter(i => i.id !== id));
    });
  }
}
```

**Template with Control Flow Blocks:**
```html
<!-- Use @if instead of *ngIf -->
@if (loading()) {
  <div class="glass-spinner">Loading...</div>
}

@if (error()) {
  <div class="glass-alert error">{{ error() }}</div>
}

<!-- Use @for instead of *ngFor -->
@for (item of items(); track item.id) {
  <app-{feature}-card [item]="item" @delete="onDelete($event)"></app-{feature}-card>
}

@empty {
  <div class="empty-state">No items found</div>
}
```

**Detail Component (Presentational) - With input():**
```typescript
import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-{feature}-detail',
  templateUrl: './{feature}-detail.component.html',
  styleUrls: ['./{feature}-detail.component.scss'],
  standalone: true
})
export class {Feature}DetailComponent {
  // Use input() and output() functions
  item = input.required<{Feature}>();
  edit = output<{Feature}>();
  delete = output<number>();
  
  onEdit() {
    this.edit.emit(this.item());
  }
  
  onDelete() {
    this.delete.emit(this.item().id);
  }
}
```

**Template with Control Flow:**
```html
@if (item(); as item) {
  <div class="glass-card">
    <h2>{{ item.name }}</h2>
    <p>{{ item.description }}</p>
    
    <div class="actions">
      <button (click)="onEdit()">Edit</button>
      <button (click)="onDelete()">Delete</button>
    </div>
  </div>
}
```

### Step 4: Create Service (using inject() pattern)

```typescript
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class {Feature}Service {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/{features}';
  
  getAll() {
    return this.http.get<{Feature}[]>(this.baseUrl);
  }
  
  getById(id: number) {
    return this.http.get<{Feature}>(`${this.baseUrl}/${id}`);
  }
  
  create(item: Create{Feature}Dto) {
    return this.http.post<{Feature}>(this.baseUrl, item);
  }
  
  update(id: number, item: Update{Feature}Dto) {
    return this.http.put<{Feature}>(`${this.baseUrl}/${id}`, item);
  }
  
  delete(id: number) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
```

**Benefits of using inject():**
- \u2705 No constructor boilerplate
- \u2705 Cleaner, more modern Angular code
- \u2705 Works in initializers
- \u2705 No need to list all dependencies

### Step 5: Configure Routing (Modern Standalone Approach)

```typescript
// In features/{feature}/lib/{feature}.routes.ts
import { Routes } from '@angular/router';

export const {FEATURE_UPPER}_ROUTES: Routes = [
  {
    path: '',
    component: {Feature}ListComponent
  },
  {
    path: 'new',
    component: {Feature}FormComponent
  },
  {
    path: ':id',
    component: {Feature}DetailComponent
  },
  {
    path: ':id/edit',
    component: {Feature}FormComponent
  }
];
```

**In app.routes.ts (main routing):**
```typescript
const routes: Routes = [
  {
    path: '{features}',
    loadChildren: () => import('./features/{feature}/lib/{feature}.routes')
      .then(routes => routes.{FEATURE_UPPER}_ROUTES)
  }
];
```

**Benefits of standalone routing:**
- ✅ No feature modules needed
- ✅ Better tree-shaking
- ✅ Simpler structure
- ✅ Modern Angular approach

### Step 6: Create Models

```typescript
export interface {Feature} {
  id: number;
  name: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface Create{Feature}Dto {
  name: string;
  // Add other fields
}

export interface Update{Feature}Dto extends Partial<Create{Feature}Dto> {}
```

### Step 7: Write Tests

Using standalone components with signals, testing is simpler:

```typescript
import { TestBed } from '@angular/core/testing';
import { {Feature}ListComponent } from './{feature}-list.component';

describe('{Feature}ListComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [{Feature}ListComponent],
      providers: [
        { provide: {Feature}Service, useValue: mockService }
      ]
    }).compileComponents();
  });

  it('should update items when signal changes', () => {
    const fixture = TestBed.createComponent({Feature}ListComponent);
    const component = fixture.componentInstance;
    
    // Signals update directly
    component.items.set(mockItems);
    fixture.detectChanges();
    
    expect(component.items().length).toBe(mockItems.length);
  });
});
```

## Quality Criteria
- Components follow single responsibility
- **Use signals for local state management**
- **Use control flow blocks (@if, @for, @switch) in templates**
- Input/output use input()/output() functions
- Services use dependency injection with inject()
- Lazy loading properly configured
- Async operations use observables
- Error handling comprehensive
- Standalone components preferred

## Verification Checklist
- [ ] Solution builds without errors: `ng build`
- [ ] Tests run: `ng test`
- [ ] **Signals used for local state**
- [ ] **Control flow blocks used in templates**
- [ ] Routing works: navigate to /{feature}
- [ ] Components render correctly
- [ ] Service API calls work
- [ ] No memory leaks
- [ ] No explicit subscriptions without takeUntil

## Edge Cases
- Handling empty lists
- Loading and error states
- Form validation for create/edit
- Concurrent requests

## References
- Angular Style Guide
- Lazy Loading Guide

## Changelog
- v1.0: Angular feature module scaffold
- v1.1: **UPDATED: Signals and Control Flow Blocks Priority**
  - Signals (not Observables) for all local state
  - Control flow blocks (@if, @for, @switch) instead of *ngIf, *ngFor
  - input()/output() instead of @Input/@Output decorators
  - Standalone components with routing instead of feature modules
  - inject() for dependency injection
  - Modern Angular 17+ patterns
