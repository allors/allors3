import { Page, Locator } from '@playwright/test';

export class MaterialDialogComponent {
  readonly page: Page;
  readonly overlay: Locator;
  readonly container: Locator;

  constructor(page: Page) {
    this.page = page;
    this.overlay = page.locator('.cdk-overlay-container');
    this.container = page.locator('mat-dialog-container');
  }

  async isOpen(): Promise<boolean> {
    return await this.container.isVisible();
  }

  async waitForOpen(): Promise<void> {
    await this.container.waitFor({ state: 'visible' });
  }

  async waitForClose(): Promise<void> {
    await this.container.waitFor({ state: 'hidden' });
  }

  async getTitle(): Promise<string> {
    const title = this.container
      .locator('h1, h2, mat-card-title, [mat-dialog-title]')
      .first();
    return (await title.textContent())?.trim() || '';
  }

  async clickButton(buttonText: string): Promise<void> {
    const button = this.container.locator('button', { hasText: buttonText });
    await button.click();
  }

  async confirm(): Promise<void> {
    await this.clickButton('OK');
  }

  async cancel(): Promise<void> {
    await this.clickButton('Cancel');
  }

  async close(): Promise<void> {
    const closeButton = this.container.locator(
      'button[mat-dialog-close], button:has(mat-icon:text("close"))'
    );
    if (await closeButton.isVisible()) {
      await closeButton.click();
    } else {
      await this.overlay.locator('.cdk-overlay-backdrop').click({ force: true });
    }
  }
}
