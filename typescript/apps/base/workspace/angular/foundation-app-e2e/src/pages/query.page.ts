import { Page, Locator } from '@playwright/test';

export class QueryPage {
  readonly page: Page;
  readonly skipInput: Locator;
  readonly takeInput: Locator;
  readonly queryButton: Locator;
  readonly totalCount: Locator;
  readonly organisationList: Locator;

  constructor(page: Page) {
    this.page = page;
    this.skipInput = page.locator('input[name="skip"]');
    this.takeInput = page.locator('input[name="take"]');
    this.queryButton = page.locator('button', { hasText: 'Query' });
    this.totalCount = page.locator('div', { hasText: 'Total:' });
    // Use hr + ul to target the organisation list specifically (after the <hr> element)
    this.organisationList = page.locator('hr + ul');
  }

  async goto() {
    await this.page.goto('/query');
  }

  async waitForQueryPage() {
    await this.page.waitForURL('**/query');
    await this.queryButton.waitFor({ state: 'visible' });
  }

  async setSkip(value: number) {
    await this.skipInput.fill(value.toString());
  }

  async setTake(value: number) {
    await this.takeInput.fill(value.toString());
  }

  async clickQuery() {
    await this.queryButton.click();
  }

  async getOrganisations(): Promise<string[]> {
    await this.organisationList.waitFor({ state: 'visible' });
    const items = this.organisationList.locator('li');
    const count = await items.count();
    const organisations: string[] = [];
    for (let i = 0; i < count; i++) {
      const text = await items.nth(i).textContent();
      if (text) organisations.push(text.trim());
    }
    return organisations;
  }

  async getTotalCount(): Promise<number> {
    const text = await this.totalCount.textContent();
    const match = text?.match(/Total:\s*(\d+)/);
    return match ? parseInt(match[1], 10) : 0;
  }

  async clickOrganisation(index: number) {
    const link = this.organisationList.locator('li a').nth(index);
    await link.click();
  }
}
