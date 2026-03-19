# SKILL: angular-component-architecture

## Purpose
Define Angular component architecture, patterns, and best practices for consistent, maintainable frontend development in GoingMy.

## Use When
- Creating new Angular components
- Reviewing component structure
- Implementing feature modules
- Refactoring component hierarchy

## Project Structure
```
src/app/
├── shared/
│   ├── components/        # Reusable components
│   ├── directives/        # Custom directives
│   ├── pipes/             # Custom pipes
│   ├── services/          # Shared services
│   └── models/            # Shared interfaces/types
├── core/
│   ├── services/          # Singleton services (auth, etc.)
│   └── guards/            # Route guards
├── features/
│   ├── user/
│   │   ├── components/
│   │   ├── services/
│   │   ├── models/
│   │   └── user-routing.module.ts
│   └── post/
│       └── ...
└── app.module.ts
```

## Component Organization

### Smart Component (Container) - Using Signals (Recommended)

**IMPORTANT: All new components should use Signals instead of Observables**

```typescript
import { Component, signal, computed, inject } from '@angular/core';
import { UserService } from './services/user.service';

@Component({
  selector: 'app-user-profile',
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.scss'],
  standalone: true  // Signals work great with standalone components
})
export class UserProfileComponent {
  private userService = inject(UserService);
  
  // Use signal for reactive state
  user = signal<User | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);
  
  // Computed signals derive from other signals
  userName = computed(() => this.user()?.name ?? 'Unknown');
  isAdmin = computed(() => this.user()?.role === 'admin');
  
  constructor() {
    this.loadUser();
  }
  
  private loadUser() {
    this.isLoading.set(true);
    this.userService.getCurrentUser().subscribe({
      next: (user) => {
        this.user.set(user);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load user');
        this.isLoading.set(false);
      }
    });
  }
}
```

**Advantages of Signals:**
- ✅ Simpler than RxJS Observables for local state
- ✅ Automatic change detection optimization
- ✅ Fine-grained reactivity (only components using signals re-render)
- ✅ Computed properties update automatically
- ✅ No unsubscribe management needed for signals

### Dumb Component (Presentational) - With input() and output()

**Use new input() and output() functions instead of @Input/@Output decorators**

```typescript
import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-user-card',
  templateUrl: './user-card.component.html',
  standalone: true
})
export class UserCardComponent {
  // Use input() instead of @Input
  user = input.required<User>();
  
  // Optional input with default
  isSelected = input(false);
  
  // Use output() instead of @Output
  userSelected = output<User>();
  
  onSelect() {
    this.userSelected.emit(this.user());
  }
}
```

**Benefits of input() and output():**
- ✅ More concise than decorators
- ✅ input.required() enforces required inputs at compile-time
- ✅ Better TypeScript support
- ✅ Cleaner component code
- ✅ Signals-compatible

## Reactive Programming - Signals First, RxJS When Needed

### Signals Pattern (Recommended for new code)
```typescript
import { Component, signal, computed, effect } from '@angular/core';

export class UserComponent {
  users = signal<User[]>([]);
  
  // Computed signals auto-update
  userCount = computed(() => this.users().length);
  
  // Effects run when signals change
  constructor() {
    effect(() => {
      console.log(`Users changed: ${this.userCount()}`);
    });
  }
  
  addUser(user: User) {
    this.users.update(users => [...users, user]);
  }
}
```

### Async Pipe with Observables (Legacy pattern)
```typescript
// Still use for service streams, but display with signals when possible
user$ = this.userService.getUser();

// Template: {{ user$ | async as user }}
```

### Unsubscription (Only needed for Observables)
```typescript
// ✅ Using takeUntilDestroyed (newer, simpler)
user$ = this.userService.getUser()
  .pipe(takeUntilDestroyed(this.destroyRef))
  .subscribe(user => this.user.set(user));
```

**Signals eliminate subscription management entirely!**

## State Management with NgRx (if applicable)

### Action
```typescript
export const loadUser = createAction(
  '[User API] Load User',
  props<{ id: number }>()
);
```

### Reducer
```typescript
const userReducer = createReducer(
  initialState,
  on(loadUser, (state) => ({ ...state, loading: true }))
);
```

## Services Pattern

### Single Responsibility
```typescript
// User service - only user-related logic
@Injectable()
export class UserService {
  constructor(private http: HttpClient) {}
  
  getUser(id: number): Observable<User> {
    return this.http.get<User>(`/api/v1/users/${id}`);
  }
}
```

### Error Handling
```typescript
getUser(id: number): Observable<User> {
  return this.http.get<User>(`/api/v1/users/${id}`).pipe(
    catchError(error => {
      console.error('Failed to load user', error);
      return throwError(() => new Error('User not found'));
    })
  );
}
```

## Glassmorphism Design Implementation

### Styling Conventions
```scss
// Design tokens
$glass-blur: 10px;
$glass-opacity: 0.8;
$glass-border: 1px solid rgba(255, 255, 255, 0.2);

// Glassmorphic component
.glass-card {
  background: rgba(255, 255, 255, $glass-opacity);
  backdrop-filter: blur($glass-blur);
  border: $glass-border;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
}
```

### Component Styling Structure
```
src/app/shared/styles/
├── _variables.scss     # Design tokens
├── _mixins.scss        # Reusable styles
├── _utilities.scss     # Utility classes
└── _glass.scss         # Glassmorphism styles
```

## Forms Implementation

### Reactive Forms (Preferred)
```typescript
export class CreateUserComponent implements OnInit {
  form: FormGroup;
  
  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      name: ['', [Validators.required, Validators.minLength(2)]]
    });
  }
}
```

## Change Detection - Signals Auto-Optimize

### Default Strategy Works Great with Signals
```typescript
@Component({
  selector: 'app-user-card'
  // No need to specify OnPush - signals auto-optimize!
})
export class UserCardComponent {
  user = input.required<User>();
  // Change detection automatically optimized
}
```

**With signals:**
- ✅ Change detection automatically optimized
- ✅ No need for OnPush strategy
- ✅ Only affected components re-render
- ✅ Performance improvements come automatically

## Lazy Loading with Standalone Components

### Modern Routing (Recommended)
```typescript
const routes: Routes = [
  {
    path: 'users',
    loadChildren: () => import('./features/user/user.routes')
      .then(routes => routes.USER_ROUTES)
  }
];

// In features/user/user.routes.ts
export const USER_ROUTES: Routes = [
  { path: '', component: UserListComponent },
  { path: ':id', component: UserDetailComponent }
];
```

### Legacy Feature Modules (Avoid for new code)
```typescript
// Avoid - use standalone components instead
const routes: Routes = [{
  path: 'users',
  loadChildren: () => import('./features/user/user.module')
    .then(m => m.UserModule)
}];
```

## Testing Expectations
- Unit tests with Jasmine/Karma
- Component testing using TestBed
- Service testing with HttpClientTestingModule
- **Test signals:** Use `patchSignal()` to update in tests
- Coverage target: > 80%

### Testing Signals
```typescript
it('should update user when signal changes', () => {
  // Arrange
  const component = TestBed.createComponent(UserComponent);
  
  // Act
  component.componentInstance.user.set(mockUser);
  
  // Assert
  expect(component.componentInstance.userName()).toBe('John Doe');
});
```

## Quality Criteria
- Components follow single responsibility
- Smart/dumb component separation clear
- **Prefer signals for local state** (not observables)
- Use control flow blocks (@if, @for, @switch) in templates
- Input/output use signal-compatible input()/output() functions
- Async operations use observables or toSignal()
- Styling follows glassmorphism design
- Error handling comprehensive

## Verification Checklist
- [ ] Components have clear selectors
- [ ] @Input/@Output properly defined
- [ ] Subscriptions properly unsubscribed
- [ ] Async pipe used instead of explicit subscriptions
- [ ] No memory leaks in component lifecycle
- [ ] Unit tests cover main logic
- [ ] Style follows design system

## References
- Angular Documentation
- RxJS Documentation
- Clean Code in Angular

## Changelog
- v1.0: Initial Angular component architecture
- v1.1: Added glassmorphism styling guidelines
- v1.2: **UPDATED: Prioritize signals and control flow blocks**
  - Signals recommended for all new local state
  - Use input()/output() instead of decorators
  - Control flow blocks for templates
  - Standalone components as default
