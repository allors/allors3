import { Page, Locator } from '@playwright/test';

export class LoginPage {
  readonly page: Page;
  readonly card: Locator;
  readonly heading: Locator;
  readonly usernameInput: Locator;
  readonly passwordInput: Locator;
  readonly signInButton: Locator;
  readonly form: Locator;

  constructor(page: Page) {
    this.page = page;
    this.card = page.locator('mat-card');
    this.heading = page.locator('mat-card-header h1');
    this.usernameInput = page.locator('input[formcontrolname="userName"]');
    this.passwordInput = page.locator('input[formcontrolname="password"]');
    this.signInButton = page.locator('mat-card-actions button');
    this.form = page.locator('form');
  }

  async goto(): Promise<void> {
    await this.page.goto('/login');
  }

  async waitForLoginPage(): Promise<void> {
    await this.page.waitForURL('**/login');
    await this.card.waitFor({ state: 'visible' });
  }

  async login(username: string, password = ''): Promise<void> {
    await this.usernameInput.fill(username);
    await this.passwordInput.fill(password);
    await this.signInButton.click();
  }

  async isDisplayed(): Promise<boolean> {
    return await this.card.isVisible();
  }
}
