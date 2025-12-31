import { Page, Locator } from '@playwright/test';

export class FetchPage {
  readonly page: Page;
  readonly organisationDetails: Locator;
  readonly relatedOrganisationsList: Locator;

  constructor(page: Page) {
    this.page = page;
    // The organisation details div contains "with owner"
    this.organisationDetails = page.locator('div', { hasText: 'with owner' }).first();
    // The related organisations list contains links to /fetch/ routes (not the nav ul)
    this.relatedOrganisationsList = page.locator('ul:has(a[href*="/fetch/"])');
  }

  async goto(id: string) {
    await this.page.goto(`/fetch/${id}`);
  }

  async waitForFetchPage() {
    await this.page.waitForURL('**/fetch/**');
  }

  async getOrganisationDetails(): Promise<string> {
    const text = await this.organisationDetails.textContent();
    return text?.trim() || '';
  }

  async getRelatedOrganisations(): Promise<string[]> {
    const items = this.relatedOrganisationsList.locator('li');
    const count = await items.count();
    const organisations: string[] = [];
    for (let i = 0; i < count; i++) {
      const text = await items.nth(i).textContent();
      if (text) organisations.push(text.trim());
    }
    return organisations;
  }

  async clickRelatedOrganisation(index: number) {
    const link = this.relatedOrganisationsList.locator('li a').nth(index);
    await link.click();
  }
}
