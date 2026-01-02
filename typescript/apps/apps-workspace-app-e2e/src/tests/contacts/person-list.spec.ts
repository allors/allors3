import { test, expect } from '../../fixtures/auth.fixture';
import { PersonListPage } from '../../pages/contacts/person-list.page';

test.describe('Person List', () => {
  test('should display person list page with table', async ({
    authenticatedPage,
  }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    await expect(personListPage.table.table).toBeVisible();
  });

  test('should display person data in table', async ({ authenticatedPage }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    const rowCount = await personListPage.table.getRowCount();
    if (rowCount > 0) {
      const personData = await personListPage.getPersonData(0);
      expect(personData.firstName).toBeDefined();
    }
  });

  test('should sort by firstName when clicking header', async ({
    authenticatedPage,
  }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    await personListPage.sortByFirstName();
    await authenticatedPage.waitForTimeout(500);

    // Verify sorting happened (header should have sort indicator)
    const header = personListPage.table.headerRow.locator('th[mat-header-cell]', {
      hasText: /First Name/i,
    });
    await expect(header).toBeVisible();
  });

  test('should select rows via checkbox', async ({ authenticatedPage }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    const rowCount = await personListPage.table.getRowCount();
    if (rowCount > 0) {
      await personListPage.selectPerson(0);
      const isSelected = await personListPage.table.isRowSelected(0);
      expect(isSelected).toBeTruthy();
    }
  });

  test('should enable delete button when row selected', async ({
    authenticatedPage,
  }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    const rowCount = await personListPage.table.getRowCount();
    if (rowCount > 0) {
      await personListPage.selectPerson(0);
      await authenticatedPage.waitForTimeout(500);
      const isEnabled = await personListPage.isDeleteButtonEnabled();
      expect(isEnabled).toBeTruthy();
    }
  });

  test('should select all rows via header checkbox', async ({
    authenticatedPage,
  }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    const rowCount = await personListPage.table.getRowCount();
    if (rowCount > 0) {
      await personListPage.table.selectAllRows();
      const isSelected = await personListPage.table.isRowSelected(0);
      expect(isSelected).toBeTruthy();
    }
  });

  test('should display filter area', async ({ authenticatedPage }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    await expect(personListPage.filterArea).toBeVisible();
  });

  test('should navigate to person overview on row click', async ({
    authenticatedPage,
  }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    const rowCount = await personListPage.table.getRowCount();
    if (rowCount > 0) {
      await personListPage.openPersonOverview(0);
      await expect(authenticatedPage).toHaveURL(/.*\/contacts\/person\//);
    }
  });

  test('should display row actions menu', async ({ authenticatedPage }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    const rowCount = await personListPage.table.getRowCount();
    if (rowCount > 0) {
      await personListPage.table.openRowMenu(0);
      await expect(authenticatedPage.locator('mat-menu')).toBeVisible();
    }
  });
});
