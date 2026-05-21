---
name: liquid-glass-styler
description: '**UI DESIGN SKILL** — Design social web app components and pages with Apple liquid glass aesthetic, generate common CSS styling and variables before code implementation, with PrimeNG integration. USE FOR: creating liquid glass effects, styling social media UI elements, prototyping page layouts with liquid glass design. DO NOT USE FOR: backend development, non-liquid glass styling, general coding tasks.'
---

# Liquid Glass Styler Skill

## Overview
This skill helps designers create stunning social web app interfaces using Apple's liquid glass aesthetic. It focuses on generating CSS for liquid glass effects, CSS variables, and designing page layouts before diving into code, with integration for PrimeNG components.

## Documents
- [liquidglass-kit.dev](https://liquidglass-kit.dev/) - A comprehensive resource for liquid glass design patterns, CSS snippets, and best practices.
- [Apple Design Resources](https://developer.apple.com/design/resources/) - Official design guidelines and assets from Apple, including liquid glass elements.
- [aethercss.lovable.app](https://aethercss.lovable.app/) - A tool for generating CSS variables and classes for liquid glass effects, with a focus on performance and accessibility.

## Workflow Steps

1. **Component Analysis**
   - Identify the UI component or page section to design
   - Consider user interaction patterns typical in social apps (feeds, profiles, interactions)
   - Determine liquid glass elements: backdrop blur, transparency, borders

2. **CSS Variables Generation**
   - Define CSS custom properties for glass effects (opacity, blur, colors)
   - Create reusable variables for consistent theming
   - Integrate with PrimeNG theme variables where applicable

3. **CSS Generation**
   - Generate base liquid glass CSS classes
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

### Glass Card Component
```css
/* Liquid Glass Card */
.liquid-glass-card {
  position: relative;
  width: 400px;
  height: 300px;
  border-radius: 28px;
  isolation: isolate;
  box-shadow: 0px 0px 21px -8px rgba(255, 255, 255, 0.3);
  cursor: pointer;
}


/* Tint and inner shadow layer */
.liquid-glass-card::before {
  content: '';
  position: absolute;
  inset: 0;
  z-index: 0;
  border-radius: 28px;
  box-shadow: inset 0 0 12px -2px rgba(255, 255, 255, 0.7);
  background-color: rgba(255, 255, 255, 0);
  pointer-events: none;
}

/* Backdrop blur and distortion layer */
.liquid-glass-card::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  border-radius: 28px;
  backdrop-filter: blur(6px);
  -webkit-backdrop-filter: blur(6px);
  filter: url(#glass-distortion);
  -webkit-filter: url(#glass-distortion);
  isolation: isolate;
  pointer-events: none;
}
```

### Liquid Glass Button
```css
.glass-button {
  width: 100%;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  color: white;
  padding: 8px 16px;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  transition: all 0.2s ease;
}

.glass-button:hover {
  background: rgba(255, 255, 255, 0.2);
}

.glass-button:focus {
  outline: none;
  box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.4);
}
```

## Quality Checks
- [ ] Glass effects enhance rather than distract from content
- [ ] Consistent with Apple's design language
- [ ] Accessible contrast ratios maintained
- [ ] Performance optimized for smooth animations
- [ ] Responsive across all device sizes

## Assets
- `templates/glass-components.css` - Pre-built liquid glass CSS classes
- `examples/social-feed-layout.html` - Sample social app page structure
- `palettes/apple-glass-colors.json` - Color schemes matching liquid glass aesthetic