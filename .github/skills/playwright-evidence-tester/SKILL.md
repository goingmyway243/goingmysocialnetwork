---
name: playwright-evidence-tester
description: '**TESTING SKILL** — Create Playwright UI automation tests with evidence capture (screenshots/videos) for test results. USE FOR: writing automated UI tests, capturing visual evidence, generating test reports. DO NOT USE FOR: API testing, manual testing, non-Playwright frameworks, production code.'
---

# Playwright Evidence Tester Skill

## Overview
This skill helps testers create comprehensive Playwright automation tests for UI testing, with built-in evidence capture (screenshots and videos) as part of test results. It focuses on generating test scripts that automatically capture visual proof of test execution.

## Workflow Steps

1. **Test Planning**
   - Define UI test scenarios and user journeys
   - Identify key interactions and expected outcomes
   - Determine evidence capture points (before/after actions, on failures)

2. **Script Writing**
   - Write Playwright test code for UI interactions
   - Implement page object models for maintainable tests
   - Add assertions for expected behavior

3. **Evidence Setup**
   - Configure screenshot capture on test steps
   - Set up video recording for full test runs
   - Define evidence naming and storage conventions

4. **Test Execution**
   - Run tests in headless or headed mode
   - Collect screenshots and videos automatically
   - Generate HTML reports with embedded evidence

5. **Result Analysis**
   - Review captured evidence for visual verification
   - Analyze test failures with screenshot context
   - Validate UI behavior through video playback

## Login Credentials
- username: `playwright-mcp`
- password: `Pass@word123`

## Common Patterns

### Basic Test with Evidence
```javascript
const { test, expect } = require('@playwright/test');

test('login flow with evidence', async ({ page }) => {
  // Capture initial state
  await page.screenshot({ path: 'evidence/login-start.png' });
  
  await page.goto('/login');
  await page.fill('#username', 'testuser');
  await page.fill('#password', 'password');
  
  // Capture before submit
  await page.screenshot({ path: 'evidence/login-filled.png' });
  
  await page.click('#submit');
  
  // Capture result
  await page.screenshot({ path: 'evidence/login-success.png' });
  
  await expect(page.locator('.dashboard')).toBeVisible();
});
```

### Video Recording Setup
```javascript
// playwright.config.js
export default {
  use: {
    video: 'on-first-retry', // or 'retain-on-failure'
  },
  reporter: [['html', { open: 'never' }]],
};
```

## Quality Checks
- [ ] Tests cover critical UI user journeys
- [ ] Evidence capture doesn't impact test performance
- [ ] Screenshots/videos clearly show test actions and results
- [ ] Test scripts are maintainable and readable
- [ ] Evidence is properly organized and accessible

## Assets
- `templates/basic-test.js` - Starter test template with evidence capture
- `configs/playwright.config.js` - Configuration for evidence recording
- `examples/login-test.js` - Complete login flow test example
- `.playwright-mcp/` - Directory for storing generated test logs and evidence