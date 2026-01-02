import { Page, Locator } from '@playwright/test';
import { MaterialTableComponent } from '../components/material-table.component';

export class OrganisationListPage {
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
    await this.page.goto('/contacts/organisations');
  }

  async waitForPage(): Promise<void> {
    await this.page.waitForURL('**/contacts/organisations');
    await this.table.waitForTableLoad();
  }

  async getTitle(): Promise<string> {
    return (await this.title.textContent())?.trim() || '';
  }

  async getOrganisationData(
    rowIndex: number
  ): Promise<{ name: string; country: string; owner: string }> {
    const row = await this.table.getRowByIndex(rowIndex);
    const cells = row.locator('td[mat-cell]');
    // Columns: select (0), name (1), country (2), owner (3), menu (4)
    return {
      name: (await cells.nth(1).textContent())?.trim() || '',
      country: (await cells.nth(2).textContent())?.trim() || '',
      owner: (await cells.nth(3).textContent())?.trim() || '',
    };
  }

  async selectOrganisation(rowIndex: number): Promise<void> {
    await this.table.selectRow(rowIndex);
  }

  async deleteSelected(): Promise<void> {
    await this.deleteButton.click();
  }

  async isDeleteButtonEnabled(): Promise<boolean> {
    return !(await this.deleteButton.isDisabled());
  }

  async openOrganisationOverview(rowIndex: number): Promise<void> {
    // Click on name column to trigger default action
    await this.table.clickRowCell(rowIndex, 1);
  }

  async sortByName(): Promise<void> {
    await this.table.clickSortHeader('Name');
  }
}
