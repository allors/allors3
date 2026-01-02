import { Page, Locator } from '@playwright/test';
import { MaterialTableComponent } from '../components/material-table.component';

export class CountryListPage {
  readonly page: Page;
  readonly breadcrumb: Locator;
  readonly title: Locator;
  readonly deleteButton: Locator;
  readonly createFab: Locator;
  readonly filterArea: Locator;
  readonly table: MaterialTableComponent;

  constructor(page: Page) {
    this.page = page;
    this.breadcrumb = page.locator('.a-breadcrumb');
    this.title = page.locator('.a-header mat-toolbar-row div.pl-3');
    this.deleteButton = page.locator('button[mattooltip="Delete"]');
    this.createFab = page.locator('a-mat-factory-fab');
    this.filterArea = page.locator('a-mat-filter');
    this.table = new MaterialTableComponent(page);
  }

  async goto(): Promise<void> {
    await this.page.goto('/contacts/countries');
  }

  async waitForPage(): Promise<void> {
    await this.page.waitForURL('**/contacts/countries');
    await this.table.waitForTableLoad();
  }

  async getTitle(): Promise<string> {
    return (await this.title.textContent())?.trim() || '';
  }

  async getCountryData(
    rowIndex: number
  ): Promise<{ isoCode: string; name: string }> {
    const row = await this.table.getRowByIndex(rowIndex);
    const cells = row.locator('td[mat-cell]');
    // Columns: select (0), isoCode (1), name (2), menu (3)
    return {
      isoCode: (await cells.nth(1).textContent())?.trim() || '',
      name: (await cells.nth(2).textContent())?.trim() || '',
    };
  }

  async selectCountry(rowIndex: number): Promise<void> {
    await this.table.selectRow(rowIndex);
  }

  async deleteSelected(): Promise<void> {
    await this.deleteButton.click();
  }

  async isDeleteButtonEnabled(): Promise<boolean> {
    return !(await this.deleteButton.isDisabled());
  }

  async openCountryEdit(rowIndex: number): Promise<void> {
    // Click on isoCode column to trigger default action (edit for countries)
    await this.table.clickRowCell(rowIndex, 1);
  }

  async sortByIsoCode(): Promise<void> {
    await this.table.clickSortHeader('Iso Code');
  }

  async sortByName(): Promise<void> {
    await this.table.clickSortHeader('Name');
  }
}
