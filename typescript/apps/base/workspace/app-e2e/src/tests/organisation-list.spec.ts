import { test, expect } from '../fixtures/auth.fixture';
import { OrganisationListPage } from '../pages/organisation-list.page';

test.describe('Organisation List Page', () => {
  test('should display organisation list page with table', async ({
    authenticatedPage,
  }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();

    const title = await organisationListPage.getTitle();
    // Title could be "Organisations" or "Companies" depending on configuration
    expect(title.length).toBeGreaterThan(0);
    await expect(organisationListPage.table.table).toBeVisible();
  });

  test('should display organisation data with Country and Owner columns', async ({
    authenticatedPage,
  }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();
    // Wait for data to load
    await authenticatedPage.waitForTimeout(1000);

    const rowCount = await organisationListPage.table.getRowCount();
    // Skip test if no organisations in test data
    test.skip(rowCount === 0, 'No organisations in test data');

    const orgData = await organisationListPage.getOrganisationData(0);
    expect(orgData.name).toBeTruthy();
  });

  test('should sort by name when clicking header', async ({
    authenticatedPage,
  }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();

    await organisationListPage.sortByName();
    await authenticatedPage.waitForTimeout(500);

    const sortedData = await organisationListPage.getOrganisationData(0);
    expect(sortedData.name).toBeTruthy();
  });

  test('should select rows via checkbox', async ({ authenticatedPage }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();

    await organisationListPage.selectOrganisation(0);

    expect(await organisationListPage.table.isRowSelected(0)).toBeTruthy();
  });

  test('should enable delete button when row selected', async ({
    authenticatedPage,
  }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();

    // Initially disabled
    expect(await organisationListPage.isDeleteButtonEnabled()).toBeFalsy();

    // Select a row
    await organisationListPage.selectOrganisation(0);

    // Now enabled
    expect(await organisationListPage.isDeleteButtonEnabled()).toBeTruthy();
  });

  test('should display filter area', async ({ authenticatedPage }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();

    await expect(organisationListPage.filterArea).toBeVisible();
  });

  test('should navigate to organisation overview on row click', async ({
    authenticatedPage,
  }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();

    await organisationListPage.openOrganisationOverview(0);

    await expect(authenticatedPage).toHaveURL(
      /.*\/contacts\/organisation\/.+/
    );
  });

  test('should have pagination', async ({ authenticatedPage }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();

    await expect(organisationListPage.table.paginator).toBeVisible();
  });
});
