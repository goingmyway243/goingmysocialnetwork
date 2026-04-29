import { Injectable, effect, inject, signal } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { updatePreset } from '@primeuix/themes';
import { DarkGlassPreset, LightGlassPreset } from '../configs/app.theme';

export type Theme = 'dark' | 'light';

const STORAGE_KEY = 'gm-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {

  // ── 1. Dependencies ──────────────────────────────────────────────
  private readonly _document = inject(DOCUMENT);

  // ── 2. State ─────────────────────────────────────────────────────
  readonly theme = signal<Theme>(this._loadStoredTheme());

  // ── 3. Effects ───────────────────────────────────────────────────
  constructor() {
    effect(() => {
      const t = this.theme();
      this._document.documentElement.setAttribute('data-theme', t);
      localStorage.setItem(STORAGE_KEY, t);
      updatePreset(t === 'dark' ? DarkGlassPreset : LightGlassPreset);
    });
  }

  // ── 4. Public API ─────────────────────────────────────────────────
  toggle(): void {
    this.theme.update(t => (t === 'dark' ? 'light' : 'dark'));
  }

  // ── 5. Private helpers ────────────────────────────────────────────
  private _loadStoredTheme(): Theme {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored === 'light' ? 'light' : 'dark';
  }
}
