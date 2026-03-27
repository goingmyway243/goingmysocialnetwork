---
applyTo: "src/GoingMy.Web/**/*.{ts,html,css,scss,json}"
---

# Frontend Development Rules — GoingMy Social Network

> **Agent**: Always load and follow these rules when working on any TypeScript, HTML, CSS/SCSS, or JSON file inside `src/GoingMy.Web/`.
> Use **Context7 MCP** to fetch up-to-date documentation for Angular 20, PrimeNG, and related libraries before generating code.

---

## Tech Stack

- **Framework**: Angular 20 (standalone components)
- **UI Library**: PrimeNG
- **State Management**: Angular Signals
- **Design System**: Apple glassmorphism aesthetic
- **Language**: TypeScript (strict mode)

---

## Project Location

```
src/GoingMy.Web/
├── src/
│   ├── app/               # Feature modules, components, services
│   ├── environments/      # Environment configuration
│   ├── styles.css         # Global styles and design tokens
│   ├── index.html
│   └── main.ts
├── angular.json
├── package.json
└── tsconfig.json
```

---

## Angular Signals — State Management

Use Angular Signals as the **primary** state mechanism. Avoid RxJS where signals suffice.

```typescript
// Prefer signals for local and feature-level state
readonly posts = signal<PostDto[]>([]);
readonly isLoading = signal(false);

// Use computed for derived state
readonly hasPosts = computed(() => this.posts().length > 0);

// Use effect for side effects
effect(() => {
  console.log('Posts updated:', this.posts().length);
});
```

- Keep signal granularity at feature/component level
- Use `computed()` for all derived state — never duplicate state
- Use `effect()` for side effects (logging, syncing to storage, etc.)
- Clean up effects in `ngOnDestroy` when using manual effect refs

---

## Code Flow Blocks Pattern

Organize component logic into discrete, named code blocks. Each block has a single responsibility.

```typescript
@Component({ ... })
export class FeedComponent implements OnDestroy {

  // ── 1. Dependencies ─────────────────────────────────────────
  private readonly postService = inject(PostService);
  private readonly router = inject(Router);

  // ── 2. State ────────────────────────────────────────────────
  readonly posts = signal<PostDto[]>([]);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);

  // ── 3. Derived State ─────────────────────────────────────────
  readonly hasPosts = computed(() => this.posts().length > 0);
  readonly isEmpty = computed(() => !this.isLoading() && !this.hasPosts());

  // ── 4. Lifecycle ─────────────────────────────────────────────
  constructor() { this.loadPosts(); }
  ngOnDestroy() { /* cleanup */ }

  // ── 5. Actions ───────────────────────────────────────────────
  loadPosts(): void { ... }
  deletePost(id: string): void { ... }

  // ── 6. Navigation ────────────────────────────────────────────
  navigateToPost(id: string): void { this.router.navigate(['/posts', id]); }
}
```

---

## PrimeNG Integration

- Use PrimeNG components for all standard UI patterns
- **Customize** components using design tokens — do not replace them
- Leverage PrimeNG's theming system for brand consistency
- Document all custom theme overrides in the component stylesheet

```typescript
// Import PrimeNG components as standalone
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  imports: [ButtonModule, CardModule, InputTextModule],
  ...
})
```

---

## Glassmorphism Design System

Apply the Apple glassmorphism aesthetic consistently across all components.

### Core Properties
```css
/* Glass card */
.glass-card {
  background: rgba(255, 255, 255, 0.12);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 16px;
}

/* Glass button */
.glass-button {
  background: rgba(255, 255, 255, 0.15);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.25);
  border-radius: 12px;
  transition: all 0.2s ease;
}
```

### Design Principles
- **Depth**: Use layered semi-transparent backgrounds to create hierarchy
- **Blur**: Apply `backdrop-filter: blur()` for depth — stronger blur = higher stack level
- **Contrast**: Ensure WCAG AA contrast ratios on all text
- **Spacing**: Follow Apple HIG spacing scale (4px base unit)
- **Typography**: Use SF Pro-inspired system fonts with appropriate weights

---

## Standalone Components

All components **must** be standalone:

```typescript
@Component({
  selector: 'app-post-card',
  standalone: true,            // Always true
  imports: [CommonModule, ButtonModule, RouterLink],
  templateUrl: './post-card.component.html',
  styleUrl: './post-card.component.css',
})
export class PostCardComponent { ... }
```

---

## TypeScript Standards

- **Strict mode** is enabled — never disable it
- Use `inject()` function instead of constructor injection for services
- Prefer `readonly` signals and properties
- Type all signal generics explicitly: `signal<PostDto[]>([])`
- Implement `OnDestroy` for cleanup when using effect refs or subscriptions
- Document complex signal flows with inline comments

---

## Service Communication

Services in `src/GoingMy.Web/src/app/` call backend APIs:

```typescript
@Injectable({ providedIn: 'root' })
export class PostService {
  private readonly http = inject(HttpClient);

  getPosts(): Observable<PostDto[]> {
    return this.http.get<PostDto[]>('/api/posts');
  }
}
```

- Pass JWT tokens via `Authorization: Bearer <token>` headers (handled by HTTP interceptor)
- Use environment files (`environments/environment.ts`) for API base URLs
- Handle errors gracefully and surface them via error signals

---

## Development Workflow

```bash
# Install dependencies
cd src/GoingMy.Web
npm install

# Start dev server
ng serve

# Generate a new component
ng generate component features/posts/components/post-card --standalone

# Run tests
ng test
```

---

## Context7 MCP — Frontend Libraries

When working on frontend code, **always** fetch current docs for:

| Library | How to resolve |
|---------|----------------|
| Angular 20 | `resolve-library-id: "angular"` |
| PrimeNG | `resolve-library-id: "primeng"` |
| Angular Signals | `resolve-library-id: "angular signals"` |
| TypeScript | `resolve-library-id: "typescript"` |
