import { defineConfig, devices } from '@playwright/test';

const BASE_URL = 'http://localhost:4200';

export default defineConfig({
  testDir: './src/tests',
  fullyParallel: false,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 2 : 0,
  workers: 1,
  reporter: [['html', { outputFolder: 'playwright-report' }]],

  use: {
    baseURL: BASE_URL,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  globalSetup: require.resolve('./src/support/global-setup'),

  timeout: 30000,
  expect: {
    timeout: 5000,
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  webServer: {
    command: 'npx nx serve base-workspace-angular-foundation-app',
    url: BASE_URL,
    reuseExistingServer: !process.env['CI'],
    timeout: 120000,
    cwd: '../../../../../',
  },
});
