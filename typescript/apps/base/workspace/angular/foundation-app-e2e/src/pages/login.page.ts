import { Page, Locator } from '@playwright/test';

export class LoginPage {
  readonly page: Page;
  readonly usernameInput: Locator;
  readonly passwordInput: Locator;
  readonly signInButton: Locator;
  readonly loginForm: Locator;
  readonly heading: Locator;

  constructor(page: Page) {
    this.page = page;
    this.usernameInput = page.locator('input[formControlName="userName"]');
    this.passwordInput = page.locator('input[formControlName="password"]');
    this.signInButton = page.locator('button[type="submit"]');
    this.loginForm = page.locator('form');
    // Target the h1 inside the form (Login heading, not the app header)
    this.heading = page.locator('form h1', { hasText: 'Login' });
  }

  async goto() {
    await this.page.goto('/login');
  }

  async login(username: string, password = '') {
    await this.usernameInput.fill(username);
    await this.passwordInput.fill(password);
    await this.signInButton.click();
  }

  async waitForLoginPage() {
    await this.page.waitForURL('**/login');
    await this.loginForm.waitFor({ state: 'visible' });
  }
}
