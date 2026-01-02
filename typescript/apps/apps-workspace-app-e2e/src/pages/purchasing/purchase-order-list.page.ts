import { Page, Locator } from '@playwright/test';
import { MaterialTableComponent } from '../../components/material-table.component';

export class PurchaseOrderListPage {
  readonly page: Page;
  readonly title: Locator;
  readonly deleteButton: Locator;
  readonly createFab: Locator;
  readonly filterArea: Locator;
  readonly table: MaterialTableComponent;

  constructor(page: Page) {
    this.page = page;
    this.title = page.locator('.a-header mat-toolbar-row div.pl-3');
    this.deleteButton = page.locator('button[mattooltip="Delete"]');
    this.createFab = page.locator('a-mat-factory-fab');
    this.filterArea = page.locator('a-mat-filter');
    this.table = new MaterialTableComponent(page);
  }

  async goto(): Promise<void> {
    await this.page.goto('/purchasing/purchaseorders');
  }

  async waitForPage(): Promise<void> {
    await this.page.waitForURL('**/purchasing/purchaseorders');
    await this.table.waitForTableLoad();
  }

  async selectRow(rowIndex: number): Promise<void> {
    await this.table.selectRow(rowIndex);
  }

  async openOverview(rowIndex: number): Promise<void> {
    await this.table.clickRowCell(rowIndex, 1);
  }
}
