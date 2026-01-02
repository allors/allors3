import { Page, Locator } from '@playwright/test';

export class MaterialTableComponent {
  readonly page: Page;
  readonly container: Locator;
  readonly table: Locator;
  readonly headerRow: Locator;
  readonly rows: Locator;
  readonly selectAllCheckbox: Locator;
  readonly paginator: Locator;

  constructor(page: Page, containerSelector = 'a-mat-table') {
    this.page = page;
    this.container = page.locator(containerSelector);
    this.table = this.container.locator('table[mat-table]');
    this.headerRow = this.table.locator('tr[mat-header-row]');
    this.rows = this.table.locator('tr[mat-row]');
    this.selectAllCheckbox = this.headerRow.locator('mat-checkbox');
    this.paginator = page.locator('a-mat-paginator');
  }

  async getRowCount(): Promise<number> {
    return await this.rows.count();
  }

  async getRowByIndex(index: number): Promise<Locator> {
    return this.rows.nth(index);
  }

  async getRowById(id: string): Promise<Locator> {
    return this.table.locator(`tr[data-allors-id="${id}"]`);
  }

  async getCellText(rowIndex: number, columnIndex: number): Promise<string> {
    const row = this.rows.nth(rowIndex);
    const cells = row.locator('td[mat-cell]');
    return (await cells.nth(columnIndex).textContent())?.trim() || '';
  }

  async clickSortHeader(columnName: string): Promise<void> {
    const header = this.headerRow.locator('th[mat-header-cell]', {
      hasText: new RegExp(columnName, 'i'),
    });
    await header.click();
  }

  async selectRow(rowIndex: number): Promise<void> {
    const row = this.rows.nth(rowIndex);
    // Click on the mat-checkbox element
    const checkbox = row.locator('mat-checkbox');
    await checkbox.click();
    // Wait for Angular change detection
    await this.page.waitForTimeout(500);
    // Force a reflow/repaint by checking if selected
    await this.page.evaluate(() => {
      // Trigger Angular change detection
      (window as any).Zone?.current?.run(() => {});
    });
    await this.page.waitForTimeout(200);
  }

  async selectAllRows(): Promise<void> {
    await this.selectAllCheckbox.click();
    await this.page.waitForTimeout(300);
  }

  async isRowSelected(rowIndex: number): Promise<boolean> {
    const row = this.rows.nth(rowIndex);
    const checkbox = row.locator('mat-checkbox');
    return await checkbox.locator('input').isChecked();
  }

  async openRowMenu(rowIndex: number): Promise<void> {
    const row = this.rows.nth(rowIndex);
    const menuButton = row.locator('button:has(mat-icon:text("more_vert"))');
    await menuButton.click();
  }

  async clickRowCell(rowIndex: number, columnIndex: number): Promise<void> {
    const row = this.rows.nth(rowIndex);
    const cells = row.locator('td[mat-cell]');
    await cells.nth(columnIndex).click();
  }

  async clickMenuAction(actionName: string): Promise<void> {
    const menuItem = this.page.locator(
      `mat-menu button[data-allors-action="${actionName}"]`
    );
    await menuItem.click();
  }

  async waitForTableLoad(): Promise<void> {
    await this.table.waitFor({ state: 'visible' });
    // Wait for at least one row to appear, or timeout after 5 seconds
    try {
      await this.rows.first().waitFor({ state: 'visible', timeout: 5000 });
    } catch {
      // Table might be empty, that's okay
    }
    await this.page.waitForTimeout(300);
  }
}
