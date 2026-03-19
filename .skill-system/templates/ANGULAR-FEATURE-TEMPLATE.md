# Angular Feature Module Template

This is a template for creating a new Angular feature module in GoingMy.

## How to Use This Template

1. Use the Angular CLI to generate the module:
```bash
ng generate module features/{feature}/{feature} --routing
```

2. Follow the directory structure below
3. Create your components, services, and models
4. Add the module to the feature routing
5. Configure lazy loading in app routing

## Directory Structure

```
src/app/features/{feature}/
├── components/
│   ├── {feature}-list/
│   ├── {feature}-detail/
│   ├── {feature}-form/
│   └── {feature}-{component}/
├── services/
│   ├── {feature}.service.ts
│   └── {feature}.service.spec.ts
├── models/
│   └── {feature}.model.ts
├── directives/
│   └── {feature}.directive.ts
├── {feature}-routing.module.ts
└── {feature}.module.ts
```

## Quick Start

```bash
# Generate module
ng generate module features/post --routing
cd src/app/features/post

# Generate components
ng generate component components/post-list
ng generate component components/post-detail
ng generate component components/post-form

# Generate service
ng generate service services/post
```

## Component Templates

### Smart Component (List)
- Handles data fetching (Observable)
- Manages state
- Emits to child components

### Presentational Component (Detail)
- Receives @Input data
- Emits @Output events
- No service dependencies

## Key Patterns

### Reactive Forms
```typescript
this.form = this.fb.group({
  title: ['', [Validators.required]],
  content: ['', [Validators.required, Validators.minLength(10)]]
});
```

### Observable Pattern
```typescript
items$ = this.service.getAll();
// Template: {{ items$ | async as items }}
```

### Unsubscribe Pattern
```typescript
private destroy$ = new Subject<void>();

ngOnInit() {
  this.service.getAll()
    .pipe(takeUntil(this.destroy$))
    .subscribe(...);
}

ngOnDestroy() {
  this.destroy$.next();
  this.destroy$.complete();
}
```

## Files Included

- **{feature}.module.ts** - Feature module definition
- **{feature}-routing.module.ts** - Route configuration
- **{feature}-list.component.ts** - List view (smart)
- **{feature}-detail.component.ts** - Detail view (presentational)
- **{feature}-form.component.ts** - Create/Edit form
- **{feature}.service.ts** - Data service
- **{feature}.model.ts** - TypeScript models

## Next Steps

1. Define your models
2. Create the service with API calls
3. Build list component
4. Build detail component
5. Build form for CRUD operations
6. Add routing
7. Write tests
8. Style with glassmorphism

## References

- [Angular Component Architecture](../skills/knowledge/angular-component-architecture.md)
- [Glassmorphism Design System](../skills/knowledge/glassmorphism-design-system.md)
- [Scaffolding Guide](../skills/scaffolding/scaffold-angular-feature-module.md)
