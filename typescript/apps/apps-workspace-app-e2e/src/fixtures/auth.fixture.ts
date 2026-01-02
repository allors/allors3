import { test as base, Page } from '@playwright/test';
import { ApiClient } from '../support/api-client';

export interface AuthFixture {
  authenticatedPage: Page;
  apiClient: ApiClient;
}

export const test = base.extend<AuthFixture>({
  apiClient: async ({}, use) => {
    const client = new ApiClient();
    await use(client);
  },

  authenticatedPage: async ({ page }, use) => {
    const apiClient = new ApiClient();

    // Reset database before each test
    await apiClient.setup('full');

    // Get auth token via API
    const token = await apiClient.login('jane@example.com');
    if (!token) {
      throw new Error('Failed to get authentication token');
    }

    // Navigate to app first to set up the storage context
    await page.goto('/login');

    // Set the token in sessionStorage (matching AuthenticationSessionStoreService)
    await page.evaluate((authToken) => {
      sessionStorage.setItem('ALLORS_JWT', authToken);
    }, token);

    // Navigate to home to trigger app with auth
    await page.goto('/');

    await use(page);
  },
});

export { expect } from '@playwright/test';
