import { test, expect } from '../fixtures/auth.fixture';
import { PersonListPage } from '../pages/person-list.page';

test.describe('Person List Page', () => {
  test('should display person list page with table', async ({
    authenticatedPage,
  }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    const title = await personListPage.getTitle();
    expect(title).toContain('People');
    await expect(personListPage.table.table).toBeVisible();
  });

  test('should display person data in table', async ({ authenticatedPage }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    const rowCount = await personListPage.table.getRowCount();
    expect(rowCount).toBeGreaterThan(0);

    const personData = await personListPage.getPersonData(0);
    expect(personData.firstName).toBeTruthy();
  });

  test('should sort by firstName when clicking header', async ({
    authenticatedPage,
  }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    // Sort ascending
    await personListPage.sortByFirstName();
    await authenticatedPage.waitForTimeout(500);

    // Sort descending
    await personListPage.sortByFirstName();
    await authenticatedPage.waitForTimeout(500);

    const sortedData = await personListPage.getPersonData(0);
    expect(sortedData.firstName).toBeTruthy();
  });

  test('should select rows via checkbox', async ({ authenticatedPage }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    await personListPage.selectPerson(0);

    expect(await personListPage.table.isRowSelected(0)).toBeTruthy();
  });

  test('should enable delete button when row selected', async ({
    authenticatedPage,
  }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    // Initially disabled
    expect(await personListPage.isDeleteButtonEnabled()).toBeFalsy();

    // Select a row
    await personListPage.selectPerson(0);

    // Now enabled
    expect(await personListPage.isDeleteButtonEnabled()).toBeTruthy();
  });

  test('should select all rows via header checkbox', async ({
    authenticatedPage,
  }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    await personListPage.table.selectAllRows();

    const rowCount = await personListPage.table.getRowCount();
    for (let i = 0; i < Math.min(rowCount, 3); i++) {
      expect(await personListPage.table.isRowSelected(i)).toBeTruthy();
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

    await personListPage.openPersonOverview(0);

    await expect(authenticatedPage).toHaveURL(/.*\/contacts\/person\/.+/);
  });

  test('should have pagination', async ({ authenticatedPage }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    await expect(personListPage.table.paginator).toBeVisible();
  });

  test('should display row actions menu', async ({ authenticatedPage }) => {
    const personListPage = new PersonListPage(authenticatedPage);
    await personListPage.goto();
    await personListPage.waitForPage();

    await personListPage.table.openRowMenu(0);

    // Menu should be visible
    const menu = authenticatedPage.locator('.mat-mdc-menu-panel, .mat-menu-panel');
    await expect(menu).toBeVisible();
  });
});
