import { Page, Locator } from '@playwright/test';

export class SalesInvoiceOverviewPage {
  readonly page: Page;
  readonly header: Locator;
  readonly title: Locator;
  readonly editButton: Locator;
  readonly panels: Locator;

  constructor(page: Page) {
    this.page = page;
    this.header = page.locator('a-mat-object-page-header, .a-header');
    this.title = page.locator('.a-header h1, .a-header mat-toolbar-row');
    this.editButton = page.locator('button[mattooltip="Edit"]');
    this.panels = page.locator('mat-tab-group');
  }

  async waitForPage(): Promise<void> {
    await this.page.waitForURL('**/sales/salesinvoice/*');
    await this.header.waitFor({ state: 'visible' });
  }

  async getTitle(): Promise<string> {
    return (await this.title.textContent())?.trim() || '';
  }
}
