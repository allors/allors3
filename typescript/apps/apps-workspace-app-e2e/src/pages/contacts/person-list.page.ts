import { Page, Locator } from '@playwright/test';
import { MaterialTableComponent } from '../../components/material-table.component';

export class PersonListPage {
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
    await this.page.goto('/contacts/people');
  }

  async waitForPage(): Promise<void> {
    await this.page.waitForURL('**/contacts/people');
    await this.table.waitForTableLoad();
  }

  async getTitle(): Promise<string> {
    return (await this.title.textContent())?.trim() || '';
  }

  async getPersonData(
    rowIndex: number
  ): Promise<{ firstName: string; lastName: string; email: string }> {
    const row = await this.table.getRowByIndex(rowIndex);
    const cells = row.locator('td[mat-cell]');
    return {
      firstName: (await cells.nth(1).textContent())?.trim() || '',
      lastName: (await cells.nth(2).textContent())?.trim() || '',
      email: (await cells.nth(3).textContent())?.trim() || '',
    };
  }

  async selectPerson(rowIndex: number): Promise<void> {
    await this.table.selectRow(rowIndex);
  }

  async deleteSelected(): Promise<void> {
    await this.deleteButton.click();
  }

  async isDeleteButtonEnabled(): Promise<boolean> {
    return !(await this.deleteButton.isDisabled());
  }

  async openPersonOverview(rowIndex: number): Promise<void> {
    await this.table.clickRowCell(rowIndex, 1);
  }

  async sortByFirstName(): Promise<void> {
    await this.table.clickSortHeader('First Name');
  }

  async sortByLastName(): Promise<void> {
    await this.table.clickSortHeader('Last Name');
  }
}
