import { Page, Locator } from '@playwright/test';

export class HomePage {
  readonly page: Page;
  readonly heading: Locator;
  readonly throttledCounter: Locator;
  readonly throttledButton: Locator;

  constructor(page: Page) {
    this.page = page;
    // Target the h1 with specific text to avoid matching header h1
    this.heading = page.locator('h1', { hasText: 'home works!' });
    this.throttledCounter = page.locator('div', {
      hasText: 'Throttled counter:',
    });
    this.throttledButton = page.locator('button[throttled]');
  }

  async goto() {
    await this.page.goto('/');
  }

  async waitForHomePage() {
    await this.page.waitForURL('/');
    await this.heading.waitFor({ state: 'visible' });
  }

  async clickThrottledButton() {
    await this.throttledButton.click();
  }

  async getThrottledCounter(): Promise<number> {
    const text = await this.throttledCounter.textContent();
    const match = text?.match(/\d+/);
    return match ? parseInt(match[0], 10) : 0;
  }
}
