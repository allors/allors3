import { test, expect } from '../../fixtures/auth.fixture';
import { CommunicationEventListPage } from '../../pages/contacts/communication-event-list.page';

test.describe('Communication Event List', () => {
  test('should display communication event list page with table', async ({
    authenticatedPage,
  }) => {
    const commEventListPage = new CommunicationEventListPage(authenticatedPage);
    await commEventListPage.goto();
    await commEventListPage.waitForPage();

    await expect(commEventListPage.table.table).toBeVisible();
  });

  test('should display filter area', async ({ authenticatedPage }) => {
    const commEventListPage = new CommunicationEventListPage(authenticatedPage);
    await commEventListPage.goto();
    await commEventListPage.waitForPage();

    await expect(commEventListPage.filterArea).toBeVisible();
  });

  test('should have pagination', async ({ authenticatedPage }) => {
    const commEventListPage = new CommunicationEventListPage(authenticatedPage);
    await commEventListPage.goto();
    await commEventListPage.waitForPage();

    await expect(commEventListPage.table.paginator).toBeVisible();
  });

  test('should select rows via checkbox', async ({ authenticatedPage }) => {
    const commEventListPage = new CommunicationEventListPage(authenticatedPage);
    await commEventListPage.goto();
    await commEventListPage.waitForPage();

    const rowCount = await commEventListPage.table.getRowCount();
    if (rowCount > 0) {
      await commEventListPage.selectEvent(0);
      const isSelected = await commEventListPage.table.isRowSelected(0);
      expect(isSelected).toBeTruthy();
    }
  });
});
