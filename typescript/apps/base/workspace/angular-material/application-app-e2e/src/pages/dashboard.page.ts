import { Page, Locator } from '@playwright/test';

export class DashboardPage {
  readonly page: Page;
  readonly toolbar: Locator;
  readonly heading: Locator;
  readonly sidenavToggle: Locator;
  readonly dashboardToolbar: Locator;

  constructor(page: Page) {
    this.page = page;
    // Main toolbar in the sidenav content area
    this.toolbar = page.locator('mat-sidenav-content mat-toolbar[color="primary"]');
    // Dashboard-specific toolbar
    this.dashboardToolbar = page.locator('.a-detail mat-toolbar.a-header');
    this.heading = page.locator('.a-detail h1');
    // Sidenav toggle can be in main toolbar or dashboard toolbar
    this.sidenavToggle = page.locator('a-mat-sidenavtoggle').first();
  }

  async goto(): Promise<void> {
    await this.page.goto('/dashboard');
  }

  async waitForDashboardPage(): Promise<void> {
    await this.page.waitForURL('**/dashboard');
    // Wait for the main sidenav container to be visible
    await this.page.locator('mat-sidenav-container').waitFor({ state: 'visible' });
  }

  async getTitle(): Promise<string> {
    return (await this.heading.textContent())?.trim() || '';
  }
}
