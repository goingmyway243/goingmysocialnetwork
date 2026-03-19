# SKILL: verify-glassmorphism-ui-consistency

## Purpose
Verify Angular UI components maintain glassmorphism design consistency, accessibility standards, and responsive design across the application.

## Use When
- Component testing
- UI review before merge
- Design system validation
- Accessibility audits

## Design Token Verification

### Color Consistency
```scss
// ✅ Good: Using design tokens
.card {
  background: $glass-light;
  border: $glass-border;
  box-shadow: $glass-shadow;
}

// ❌ Bad: Hard-coded values
.card {
  background: rgba(255, 255, 255, 0.75);
  border: 1px solid rgba(255, 255, 255, 0.18);
}
```

**Verification:**
- [ ] No hard-coded colors in components
- [ ] All colors from design tokens
- [ ] Dark mode variants defined
- [ ] Sufficient color contrast (WCAG AA: 4.5:1)

### Typography Consistency
```typescript
// ✅ Good: Using design system classes
<h1 class="text-2xl font-semibold">Profile</h1>
<p class="text-sm text-neutral-500">Bio section</p>

// ❌ Bad: Inline styles
<h1 style="font-size: 32px; font-weight: 600;">Profile</h1>
```

**Verification:**
- [ ] Font sizes follow scale ($font-size-xs to $font-size-3xl)
- [ ] Font weights from defined scale
- [ ] Line heights consistent
- [ ] No inline font styles

### Spacing Consistency
```scss
// ✅ Good: Using spacing scale
.card {
  padding: $spacing-lg;
  margin-bottom: $spacing-xl;
  gap: $spacing-md;
}

// ❌ Bad: Random values
.card {
  padding: 17px;
  margin-bottom: 22px;
  gap: 11px;
}
```

**Verification:**
- [ ] Use spacing-xs (4px) to spacing-3xl (32px)
- [ ] No arbitrary pixel values
- [ ] Gutters and margins aligned to grid

## Component Implementation Verification

### Glass Card Component
```typescript
// ✅ Correct glassmorphism implementation
<div class="glass-card">
  <h2>Card Title</h2>
  <p>Content with proper spacing</p>
</div>

// CSS
.glass-card {
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  padding: 16px;
}
```

**Checklist:**
- [ ] Background has opacity (0.8)
- [ ] Backdrop filter blur applied (10px)
- [ ] Border shows glass reflection
- [ ] Shadow subtle but visible
- [ ] Border radius moderate (12px)
- [ ] Padding follows scale

### Interactive State Verification
```scss
// ✅ Complete glass button states
.glass-button {
  // Default
  background: rgba(0, 122, 255, 0.8);
  transition: all 0.3s ease;
  
  // Hover
  &:hover {
    background: rgba(0, 122, 255, 0.9);
    box-shadow: 0 12px 40px rgba(0, 122, 255, 0.2);
  }
  
  // Active
  &:active {
    transform: scale(0.98);
  }
  
  // Focus
  &:focus {
    outline: 2px solid rgba(0, 122, 255, 0.3);
    outline-offset: 2px;
  }
  
  // Disabled
  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
}
```

**Verification:**
- [ ] Hover state distinct
- [ ] Active state responsive
- [ ] Focus state visible (keyboard navigation)
- [ ] Disabled state clear
- [ ] Transitions smooth (200-300ms)

## Responsive Design Verification

### Breakpoints
```scss
$breakpoints: (
  'xs': 320px,   // Phone
  'sm': 576px,   // Landscape phone
  'md': 768px,   // Tablet
  'lg': 992px,   // Desktop
  'xl': 1200px   // Large desktop
);

// Usage
@media (min-width: map-get($breakpoints, 'md')) {
  .card {
    width: 50%;
  }
}
```

**Test Scenarios:**
- [ ] Mobile (320px) - single column
- [ ] Tablet (768px) - two columns
- [ ] Desktop (1200px) - three columns
- [ ] All touch targets ≥ 44x44px

### ViewPort Testing
```html
<!-- ✅ Correct viewport meta -->
<meta name="viewport" 
  content="width=device-width, initial-scale=1.0, 
    maximum-scale=5.0, user-scalable=yes">
```

**Verification:**
- [ ] Responsive grid working
- [ ] Images scale properly
- [ ] Text readable on all sizes
- [ ] No horizontal scrolling
- [ ] Touch interactions work well

## Accessibility (A11y) Verification

### Contrast Checker
```scss
// ✅ Good: 7:1 contrast ratio
.text-on-glass {
  color: $neutral-900;           // Dark text
  background: $glass-light;      // Light background
  // Ratio: 7:1 (exceeds WCAG AAA)
}

// ❌ Bad: 2:1 contrast ratio (below standard)
.poor-contrast {
  color: $neutral-500;           // Medium gray
  background: $glass-light;      // Light background
}
```

**Testing Tools:**
- WebAIM Contrast Checker
- axe DevTools Chrome extension
- WAVE Web Accessibility Evaluation Tool

### Keyboard Navigation
```typescript
// ✅ Keyboard accessible component
<button 
  tabindex="0"
  [disabled]="isDisabled"
  (keydown.enter)="onSelect()"
  (click)="onSelect()">
  Click or Press Enter
</button>
```

**Verification:**
- [ ] Tab order logical
- [ ] Focus visible on all interactive elements
- [ ] No keyboard traps
- [ ] Forms navigable with Tab key
- [ ] Escape closes modals

### Screen Reader Support
```html
<!-- ✅ Good screen reader support -->
<button 
  aria-label="Delete post"
  [disabled]="isDeleting"
  [attr.aria-busy]="isDeleting">
  <i class="icon-trash" aria-hidden="true"></i>
</button>

<!-- Image with description -->
<img 
  src="avatar.jpg" 
  alt="User profile photo for John Doe">
```

**Verification:**
- [ ] Alt text for all images
- [ ] ARIA labels on icons
- [ ] Landmarks defined (header, nav, main)
- [ ] Form labels associated with inputs
- [ ] Error messages linked to form fields

### Form Accessibility
```html
<!-- ✅ Accessible form structure -->
<form>
  <label for="email">Email Address</label>
  <input 
    id="email" 
    type="email" 
    required
    aria-required="true"
    aria-describedby="email-help">
  <p id="email-help" class="text-xs">We'll never share your email</p>
  
  <span 
    role="alert" 
    *ngIf="emailError$ | async as error">
    {{ error }}
  </span>
</form>
```

## Dark Mode Verification

### Color Scheme Support
```scss
// Light mode (default)
.card {
  background: rgba(255, 255, 255, 0.8);
  color: $neutral-900;
}

// Dark mode
@media (prefers-color-scheme: dark) {
  .card {
    background: rgba(30, 30, 30, 0.8);
    color: $neutral-100;
  }
}
```

**Verification:**
- [ ] All components have dark variants
- [ ] Transitions smooth between modes
- [ ] Contrast maintained in dark mode
- [ ] No hard-coded colors

## Performance Verification

### CSS Efficiency
```scss
// ✅ Efficient
.glass-effect {
  backdrop-filter: blur(10px);
}

// ❌ Avoid: Multiple filters impact performance
.heavy-effect {
  backdrop-filter: blur(10px) brightness(0.9) saturate(1.2);
}
```

**Checklist:**
- [ ] No excessive blur filters (max 10px)
- [ ] Minimal shadow layers
- [ ] Hardware acceleration used (transform, opacity)
- [ ] CSS animations, not JS animations

### Image Optimization
- [ ] Images compressed for web
- [ ] Responsive images using srcset
- [ ] Lazy loading for below-fold images
- [ ] WebP format with fallbacks

## Quality Criteria
- All components use design tokens
- Glassmorphism effects consistent
- Accessibility standards met (WCAG AA)
- Responsive across all breakpoints
- Dark mode fully supported
- Performance optimized

## Verification Checklist Template

```markdown
## UI Consistency Review

### Design Tokens
- [ ] No hard-coded colors
- [ ] Typography from scale
- [ ] Spacing aligned to grid
- [ ] Border radius consistent

### Component Quality
- [ ] Glass effect properly applied
- [ ] Interactive states complete
- [ ] Transitions smooth
- [ ] No visual glitches

### Responsive Design
- [ ] Mobile layout correct
- [ ] Tablet layout correct
- [ ] Desktop layout correct
- [ ] Touch targets adequate

### Accessibility
- [ ] Color contrast adequate (4.5:1)
- [ ] Keyboard navigation works
- [ ] Screen reader compatible
- [ ] Dark mode supported

### Decision
- [ ] ✅ APPROVED
- [ ] ⚠️ REQUEST CHANGES
- [ ] ❌ REJECT
```

## References
- Apple Design Guidelines
- WCAG 2.1 Guidelines
- Material Design System

## Changelog
- v1.0: Glassmorphism UI consistency verification
