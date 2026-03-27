const { test, expect } = require('@playwright/test');

test.describe('Login Flow Tests', () => {
  test('successful login with evidence capture', async ({ page }) => {
    // Start evidence collection
    test.setTimeout(60000); // Extended timeout for evidence

    // Navigate to login page
    await page.goto('/login');

    // Capture login page
    await page.screenshot({
      path: 'evidence/login-page.png',
      fullPage: true
    });

    // Fill login form
    await page.fill('#username', 'testuser@example.com');
    await page.fill('#password', 'securepassword123');

    // Capture filled form
    await page.screenshot({
      path: 'evidence/login-form-filled.png'
    });

    // Submit login
    await page.click('#login-button');

    // Wait for navigation or success indicator
    await page.waitForURL('**/dashboard');

    // Capture dashboard after login
    await page.screenshot({
      path: 'evidence/dashboard-after-login.png',
      fullPage: true
    });

    // Verify successful login
    await expect(page.locator('.welcome-message')).toContainText('Welcome, testuser');
    await expect(page.locator('.dashboard-content')).toBeVisible();
  });

  test('login with invalid credentials', async ({ page }) => {
    await page.goto('/login');

    // Capture initial state
    await page.screenshot({ path: 'evidence/invalid-login-start.png' });

    // Enter invalid credentials
    await page.fill('#username', 'invalid@example.com');
    await page.fill('#password', 'wrongpassword');

    await page.click('#login-button');

    // Capture error state
    await page.screenshot({ path: 'evidence/invalid-login-error.png' });

    // Verify error message
    await expect(page.locator('.error-message')).toBeVisible();
    await expect(page.locator('.error-message')).toContainText('Invalid credentials');
  });

  test('login form validation', async ({ page }) => {
    await page.goto('/login');

    // Try to submit empty form
    await page.click('#login-button');

    // Capture validation errors
    await page.screenshot({ path: 'evidence/validation-errors.png' });

    // Verify validation messages
    await expect(page.locator('#username:invalid')).toBeTruthy();
    await expect(page.locator('#password:invalid')).toBeTruthy();
  });
});