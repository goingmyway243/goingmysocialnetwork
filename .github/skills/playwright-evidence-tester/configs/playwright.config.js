// playwright.config.js - Configuration for evidence capture
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  outputDir: './test-results',

  // Evidence capture configuration
  use: {
    // Screenshots on failure
    screenshot: 'only-on-failure',

    // Video recording
    video: {
      mode: 'retain-on-failure', // or 'on' for all tests, 'off' to disable
      size: { width: 1280, height: 720 }
    },

    // Trace collection
    trace: 'retain-on-failure',
  },

  // Reporter with evidence
  reporter: [
    ['html', {
      open: 'never',
      outputFolder: './playwright-report'
    }],
    ['json', {
      outputFile: './test-results/results.json'
    }]
  ],

  // Projects for different browsers
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],
});