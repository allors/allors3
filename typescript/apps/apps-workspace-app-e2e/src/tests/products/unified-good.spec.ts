import { test, expect } from '../../fixtures/auth.fixture';
import { UnifiedGoodListPage } from '../../pages/products/unified-good-list.page';

test.describe('Unified Good List', () => {
  test('should display unified good list page with table', async ({
    authenticatedPage,
  }) => {
    const listPage = new UnifiedGoodListPage(authenticatedPage);
    await listPage.goto();
    await listPage.waitForPage();

    await expect(listPage.table.table).toBeVisible();
  });

  test('should display filter area', async ({ authenticatedPage }) => {
    const listPage = new UnifiedGoodListPage(authenticatedPage);
    await listPage.goto();
    await listPage.waitForPage();

    await expect(listPage.filterArea).toBeVisible();
  });

  test('should have pagination', async ({ authenticatedPage }) => {
    const listPage = new UnifiedGoodListPage(authenticatedPage);
    await listPage.goto();
    await listPage.waitForPage();

    await expect(listPage.table.paginator).toBeVisible();
  });

  test('should select rows via checkbox', async ({ authenticatedPage }) => {
    const listPage = new UnifiedGoodListPage(authenticatedPage);
    await listPage.goto();
    await listPage.waitForPage();

    const rowCount = await listPage.table.getRowCount();
    if (rowCount > 0) {
      await listPage.selectRow(0);
      const isSelected = await listPage.table.isRowSelected(0);
      expect(isSelected).toBeTruthy();
    }
  });

  test('should navigate to overview on row click', async ({
    authenticatedPage,
  }) => {
    const listPage = new UnifiedGoodListPage(authenticatedPage);
    await listPage.goto();
    await listPage.waitForPage();

    const rowCount = await listPage.table.getRowCount();
    if (rowCount > 0) {
      await listPage.openOverview(0);
      await expect(authenticatedPage).toHaveURL(/.*\/products\/unifiedgood\//);
    }
  });
});
