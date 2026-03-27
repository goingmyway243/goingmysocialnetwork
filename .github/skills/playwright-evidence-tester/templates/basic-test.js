const { test, expect } = require('@playwright/test');

test.describe('Basic UI Test with Evidence', () => {
  test('should complete basic interaction with screenshots', async ({ page }) => {
    // Navigate to page
    await page.goto('/');

    // Capture initial page state
    await page.screenshot({
      path: 'evidence/test-start.png',
      fullPage: true
    });

    // Perform action
    await page.click('.some-button');

    // Capture after action
    await page.screenshot({
      path: 'evidence/after-click.png',
      fullPage: true
    });

    // Verify result
    await expect(page.locator('.result')).toBeVisible();

    // Capture final state
    await page.screenshot({
      path: 'evidence/test-complete.png',
      fullPage: true
    });
  });
});