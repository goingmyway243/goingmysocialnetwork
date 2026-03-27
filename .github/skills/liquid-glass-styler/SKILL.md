---
name: liquid-glass-styler
description: '**UI DESIGN SKILL** — Design social web app components and pages with Apple liquid glass aesthetic, generate common CSS styling and variables before code implementation, with PrimeNG integration. USE FOR: creating glassmorphism effects, styling social media UI elements, prototyping page layouts with liquid glass design. DO NOT USE FOR: backend development, non-glassmorphism styling, general coding tasks.'
---

# Liquid Glass Styler Skill

## Overview
This skill helps designers create stunning social web app interfaces using Apple's liquid glass aesthetic. It focuses on generating CSS for glassmorphism effects, CSS variables, and designing page layouts before diving into code, with integration for PrimeNG components.

## Workflow Steps

1. **Component Analysis**
   - Identify the UI component or page section to design
   - Consider user interaction patterns typical in social apps (feeds, profiles, interactions)
   - Determine glassmorphism elements: backdrop blur, transparency, borders

2. **CSS Variables Generation**
   - Define CSS custom properties for glass effects (opacity, blur, colors)
   - Create reusable variables for consistent theming
   - Integrate with PrimeNG theme variables where applicable

3. **CSS Generation**
   - Generate base glassmorphism CSS classes
   - Apply liquid glass effects: blur, opacity, gradients
   - Ensure responsive design principles

4. **Layout Design**
   - Sketch page structure with glass elements
   - Position components for optimal visual hierarchy
   - Consider mobile-first responsive layouts

5. **Styling Refinement**
   - Adjust colors, shadows, and borders for Apple-like polish
   - Test visual consistency across different content types
   - Optimize for performance (minimal heavy effects)

## Common Patterns

### CSS Variables for Glass Effects
```css
:root {
  --glass-bg: rgba(255, 255, 255, 0.1);
  --glass-border: rgba(255, 255, 255, 0.2);
  --glass-blur: blur(10px);
  --glass-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  --glass-radius: 16px;
}
```

### Glass Card Component
```css
.glass-card {
  background: var(--glass-bg);
  backdrop-filter: var(--glass-blur);
  border: 1px solid var(--glass-border);
  border-radius: var(--glass-radius);
  box-shadow: var(--glass-shadow);
}
```

### Liquid Glass Button
```css
.liquid-button {
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.2), rgba(255, 255, 255, 0.05));
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 20px;
  transition: all 0.3s ease;
}
```

## Quality Checks
- [ ] Glass effects enhance rather than distract from content
- [ ] Consistent with Apple's design language
- [ ] Accessible contrast ratios maintained
- [ ] Performance optimized for smooth animations
- [ ] Responsive across all device sizes

## Assets
- `templates/glass-components.css` - Pre-built glassmorphism CSS classes
- `examples/social-feed-layout.html` - Sample social app page structure
- `palettes/apple-glass-colors.json` - Color schemes matching liquid glass aesthetic