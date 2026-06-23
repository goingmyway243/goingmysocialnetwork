import { test, expect } from '@playwright/test';

/**
 * ═══════════════════════════════════════════════════════════════
 *  🎯 DAY 1 PLAYWRIGHT PRACTICE
 *  ═══════════════════════════════════════════════════════════════
 *
 *  Goal: Learn selectors, navigation, and evidence capture
 *        without needing actual login credentials.
 *
 *  What you'll practice:
 *    1. Page navigation (goto, URL matching)
 *    2. Selector strategies (role, text, locator)
 *    3. Waiting for elements (toBeVisible, toHaveURL)
 *    4. Screenshots as evidence
 *    5. Reading test reports
 */

// ─────────────────────────────────────────────────────────────
// 📌 TEST 1: App loads and initiates OAuth flow
// ─────────────────────────────────────────────────────────────
test('should load homepage and detect unauthenticated state', async ({ page }) => {
  await page.goto('/');

  await expect(page).toHaveURL(/signin-oidc|connect\/authorize/, {
    timeout: 3000
  });
});

// ─────────────────────────────────────────────────────────────
// 📌 TEST 2: Dashboard route redirects to auth
// ─────────────────────────────────────────────────────────────
test('should redirect /dashboard to auth when not logged in', async ({ page }) => {
  // Try to navigate directly to dashboard
  await page.goto('/dashboard');
  
  // Should redirect to auth flow
  await expect(page).toHaveURL(/signin-oidc|connect\/authorize/, { timeout: 5000 });
  
  // Capture the auth flow initiation
  await page.screenshot({ path: 'test-results/screenshots/02-dashboard-redirect.png' });
});

// ─────────────────────────────────────────────────────────────
// 📌 TEST 3: Profile route requires auth
// ─────────────────────────────────────────────────────────────
test('should protect /dashboard/profile route with auth guard', async ({ page }) => {
  // Try to access protected route
  await page.goto('/dashboard/profile');
  
  // Auth guard should redirect to OAuth
  await expect(page).toHaveURL(/signin-oidc|connect\/authorize/, { timeout: 5000 });
});

// ─────────────────────────────────────────────────────────────
// 📌 TEST 4: Learn selector strategies (bonus practice)
// ─────────────────────────────────────────────────────────────
// This test demonstrates different selector patterns you can use:
//   - role selectors (button, link, heading)
//   - text-based selectors
//   - placeholder/aria-label locators
//   - CSS classes
test('selector strategy examples (educational)', async ({ page }) => {
  // Note: This test documents selectors from the app,
  // but doesn't assert anything since we're not logged in.
  // After you implement login, you can uncomment and test these:
  
  // Example selectors found in your app:
  // const searchBox = page.getByPlaceholder('Search for creators, inspirations, and projects');
  // const discoverSearch = page.getByPlaceholder('Search people, posts...');
  // const logoutBtn = page.getByRole('button', { name: /logout|sign out/i });
  // const profileLink = page.getByRole('link', { name: /profile/i });
  
  // Screenshot current page for reference
  await page.goto('/');
  await page.waitForLoadState('domcontentloaded');
  await page.screenshot({ path: 'test-results/screenshots/04-selector-reference.png' });
});

// ─────────────────────────────────────────────────────────────
// 📌 TEST 5: Practice - Capture evidence on URL changes
// ─────────────────────────────────────────────────────────────
test('multiple screenshots throughout navigation', async ({ page }) => {
  // Step 1: Start at root
  await page.goto('/');
  await page.screenshot({ path: 'test-results/screenshots/05-step1-root.png' });
  
  // Step 2: Wait for redirect
  await page.waitForURL(/signin-oidc|connect\/authorize/, { timeout: 5000 });
  await page.screenshot({ path: 'test-results/screenshots/05-step2-redirected.png' });
  
  // Step 3: Check final URL
  const finalUrl = page.url();
  console.log('Final URL:', finalUrl);
  expect(finalUrl).toContain('signin-oidc');
});