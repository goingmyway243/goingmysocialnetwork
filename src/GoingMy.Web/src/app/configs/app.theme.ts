import { definePreset } from '@primeuix/themes';
import Lara from '@primeuix/themes/lara';

/**
 * Liquid Glass Design System - PrimeNG Theme Integration
 * 
 * This preset extends the Lara theme with glassmorphism tokens that map
 * to our CSS variables defined in styles.css. It ensures consistent glass
 * effects across all PrimeNG components used in the dashboard.
 * 
 * Note: Specific component styling is handled via CSS overrides in styles.css
 * to maintain full control over glassmorphism effects.
 */
const MyPreset = definePreset(Lara, {
  semantic: {
    colorScheme: {
      light: {
        primary: {
          color: 'var(--color-accent-blue)',
          hoverColor: 'var(--color-accent-purple)',
          activeColor: 'var(--color-accent-blue)'
        },
        surface: {
          0: 'var(--surface-ground)',
          50: 'var(--glass-surface-bg)',
          100: 'var(--glass-elevated-bg)',
          200: 'var(--glass-overlay-bg)',
          300: 'var(--glass-surface-hover)',
          400: 'var(--glass-elevated-hover)',
          500: 'var(--glass-overlay-hover)',
          600: 'var(--glass-surface-active)',
          700: 'var(--glass-elevated-active)',
          800: 'rgba(255, 255, 255, 0.3)',
          900: 'rgba(255, 255, 255, 0.4)',
          950: 'rgba(255, 255, 255, 0.5)'
        }
      },
      dark: {
        primary: {
          color: 'var(--color-accent-blue)',
          hoverColor: 'var(--color-accent-purple)',
          activeColor: 'var(--color-accent-blue)'
        },
        surface: {
          0: 'var(--surface-ground)',
          50: 'var(--glass-surface-bg)',
          100: 'var(--glass-elevated-bg)',
          200: 'var(--glass-overlay-bg)',
          300: 'var(--glass-surface-hover)',
          400: 'var(--glass-elevated-hover)',
          500: 'var(--glass-overlay-hover)',
          600: 'var(--glass-surface-active)',
          700: 'var(--glass-elevated-active)',
          800: 'rgba(255, 255, 255, 0.3)',
          900: 'rgba(255, 255, 255, 0.4)',
          950: 'rgba(255, 255, 255, 0.5)'
        }
      }
    },
    // Global styling properties
    transitionDuration: '0.3s',
    borderRadius: {
      none: '0',
      xs: '0.25rem',
      sm: '0.5rem',
      md: '0.75rem',
      lg: '1rem',
      xl: '1.5rem'
    }
  }
});

// ── PrimeNG v20 Pass Through (pt) Configurations ──────────────────
// Replaces deprecated ::ng-deep styling for PrimeNG components.
// These objects define inline styles for component internals
// without breaking Angular's view encapsulation.

// Button Styles
export const GLASS_BTN_PT = {
  root: {
    style: {
      background: 'var(--glass-elevated-bg)',
      backdropFilter: 'blur(16px)',
      WebkitBackdropFilter: 'blur(16px)',
      border: '1px solid var(--glass-border-strong)',
      color: '#ffffff',
      borderRadius: '20px',
      padding: '0.5rem 1.25rem',
      fontWeight: '600',
      transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
      cursor: 'pointer'
    }
  }
};

export const FOLLOW_BTN_PT = {
  root: {
    style: {
      background: 'linear-gradient(135deg, #60a5fa, #a78bfa)',
      border: 'none',
      color: '#ffffff',
      borderRadius: '20px',
      padding: '0.5rem 1.25rem',
      fontWeight: '600',
      transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
      cursor: 'pointer'
    }
  }
};

export const CANCEL_BTN_PT = {
  root: {
    style: {
      color: 'rgba(255, 255, 255, 0.6)',
      background: 'transparent',
      border: 'none',
      cursor: 'pointer'
    }
  }
};

export const SAVE_BTN_PT = {
  root: {
    style: {
      background: 'linear-gradient(135deg, #60a5fa, #a78bfa)',
      border: 'none',
      color: '#ffffff',
      borderRadius: '10px',
      padding: '0.5rem 1.25rem',
      fontWeight: '600',
      transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
      cursor: 'pointer'
    }
  }
};

// Form Controls
export const SELECT_PT = {
  root: {
    style: {
      background: 'var(--glass-surface-bg)',
      border: '1px solid var(--glass-border-base)',
      borderRadius: '10px',
      width: '100%',
      color: '#ffffff'
    }
  },
  label: { style: { color: '#ffffff' } }
};

export default MyPreset;