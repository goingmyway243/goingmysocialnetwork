# SKILL: glassmorphism-design-system

## Purpose
Define the Apple glassmorphism design system for GoingMy UI, including components, design tokens, and implementation guidelines for consistent, premium UI.

## Use When
- Creating UI components
- Designing new features
- Implementing design specs
- Establishing brand consistency
- Building component library

## Design Philosophy

Glassmorphism combines:
- **Translucency** - Semi-transparent backgrounds
- **Frosted Glass Effect** - Blur and layering
- **Subtle Shadows** - Depth perception
- **Layering** - Background image showing through
- **Minimalism** - Clean, uncluttered design

## Design Tokens

### Colors
```scss
// Primary Colors
$brand-primary: #007AFF;      // iOS blue
$brand-secondary: #34C759;    // iOS green

// Neutral Colors
$neutral-100: #F9F9F9;        // Lightest
$neutral-200: #F2F2F7;
$neutral-500: #999999;        // Medium gray
$neutral-900: #1D1D1D;        // Darkest

// Glass Colors (with alpha)
$glass-light: rgba(255, 255, 255, 0.8);
$glass-dark: rgba(30, 30, 30, 0.8);

// Semantic Colors
$success: #34C759;
$warning: #FF9500;
$error: #FF3B30;
```

### Typography
```scss
// Font Stack
$font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto;

// Font Sizes
$font-size-xs: 12px;          // Small labels
$font-size-sm: 14px;          // Body text
$font-size-base: 16px;        // Standard
$font-size-lg: 18px;          // Subheading
$font-size-xl: 20px;          // Heading 3
$font-size-2xl: 24px;         // Heading 2
$font-size-3xl: 28px;         // Heading 1

// Font Weights
$font-weight-light: 300;
$font-weight-regular: 400;
$font-weight-medium: 500;
$font-weight-semibold: 600;
$font-weight-bold: 700;
```

### Spacing Scale
```scss
$spacing-xs: 4px;
$spacing-sm: 8px;
$spacing-md: 12px;
$spacing-lg: 16px;
$spacing-xl: 20px;
$spacing-2xl: 24px;
$spacing-3xl: 32px;
```

### Glass Effect
```scss
$glass-blur: 10px;
$glass-opacity-light: 0.8;
$glass-opacity-dark: 0.6;
$glass-border: 1px solid rgba(255, 255, 255, 0.2);
$glass-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
```

## Core Components

### Glass Card
```scss
.glass-card {
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  padding: $spacing-lg;
}
```

### Glass Button
- Subtle background with glass effect
- Hover state with increased opacity
- Active state with slight scaling

```scss
.glass-button {
  background: rgba(0, 122, 255, 0.8);
  backdrop-filter: blur(10px);
  color: white;
  border: none;
  border-radius: 8px;
  padding: $spacing-md $spacing-lg;
  transition: all 0.3s ease;
  
  &:hover {
    background: rgba(0, 122, 255, 0.9);
    transform: translateY(-2px);
    box-shadow: 0 12px 40px rgba(0, 122, 255, 0.2);
  }
}
```

### Glass Input Field
- Minimal borders, emphasis on content
- Focus state with enhanced glass effect

```scss
.glass-input {
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 8px;
  padding: $spacing-md;
  font-size: $font-size-sm;
  
  &:focus {
    outline: none;
    background: rgba(255, 255, 255, 0.9);
    box-shadow: 0 0 0 2px rgba(0, 122, 255, 0.1);
  }
}
```

## Layout System

### Grid
- 12-column responsive grid
- Breakpoints: xs(320px), sm(576px), md(768px), lg(992px), xl(1200px)

### Spacing Grid
- 4px base unit
- Multiples: 4, 8, 12, 16, 20, 24, 32, 40, 48

## Dark Mode Support

```scss
@media (prefers-color-scheme: dark) {
  .glass-card {
    background: rgba(30, 30, 30, 0.8);
    border: 1px solid rgba(255, 255, 255, 0.1);
  }
}
```

## Accessibility

- Color contrast: WCAG AA minimum (4.5:1 for text)
- Focus indicators visible
- Keyboard navigation support
- Semantic HTML structure

## Motion & Animation

- Easing: cubic-bezier(0.4, 0.0, 0.2, 1) (Material standard)
- Duration: 200ms for micro interactions, 300ms for transitions
- No animation on prefers-reduced-motion

```scss
.smooth-transition {
  transition: all 0.3s cubic-bezier(0.4, 0.0, 0.2, 1);
}
```

## Quality Criteria
- All interactive elements have clear focus states
- Color contrast meets WCAG AA
- Components responsive across breakpoints
- Glass effects subtle, not distracting
- Consistent spacing and alignment
- Dark mode properly supported

## Verification Checklist
- [ ] Colors match design tokens
- [ ] Typography hierarchy clear
- [ ] Spacing consistent with grid
- [ ] Glass effects applied correctly
- [ ] Hover/active states defined
- [ ] Dark mode tested
- [ ] Accessibility standards met
- [ ] Motion reduced for accessibility setting

## Edge Cases
- Very light backgrounds reducing glass effect visibility
- Low-end devices struggling with multiple blur effects
- Dark mode contrast issues
- Mobile touch targets (min 44x44px)

## References
- Apple Design Guidelines
- Human Interface Guidelines
- Glass UI Design Principles

## Changelog
- v1.0: Initial glassmorphism design system
