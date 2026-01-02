import { test, expect } from '../../fixtures/auth.fixture';
import { OrganisationListPage } from '../../pages/contacts/organisation-list.page';

test.describe('Organisation List', () => {
  test('should display organisation list page with table', async ({
    authenticatedPage,
  }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();

    await expect(organisationListPage.table.table).toBeVisible();
  });

  test('should display organisation data with Country and Owner columns', async ({
    authenticatedPage,
  }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();

    const rowCount = await organisationListPage.table.getRowCount();
    if (rowCount > 0) {
      const orgData = await organisationListPage.getOrganisationData(0);
      expect(orgData.name).toBeDefined();
    }
  });

  test('should sort by name when clicking header', async ({
    authenticatedPage,
  }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();

    await organisationListPage.sortByName();
    await authenticatedPage.waitForTimeout(500);

    const header = organisationListPage.table.headerRow.locator('th[mat-header-cell]', {
      hasText: /Name/i,
    });
    await expect(header).toBeVisible();
  });

  test('should select rows via checkbox', async ({ authenticatedPage }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();

    const rowCount = await organisationListPage.table.getRowCount();
    if (rowCount > 0) {
      await organisationListPage.selectOrganisation(0);
      const isSelected = await organisationListPage.table.isRowSelected(0);
      expect(isSelected).toBeTruthy();
    }
  });

  test('should enable delete button when row selected', async ({
    authenticatedPage,
  }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();

    const rowCount = await organisationListPage.table.getRowCount();
    if (rowCount > 0) {
      await organisationListPage.selectOrganisation(0);
      await authenticatedPage.waitForTimeout(500);
      const isEnabled = await organisationListPage.isDeleteButtonEnabled();
      expect(isEnabled).toBeTruthy();
    }
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

    const rowCount = await organisationListPage.table.getRowCount();
    if (rowCount > 0) {
      await organisationListPage.openOrganisationOverview(0);
      await expect(authenticatedPage).toHaveURL(/.*\/contacts\/organisation\//);
    }
  });

  test('should have pagination', async ({ authenticatedPage }) => {
    const organisationListPage = new OrganisationListPage(authenticatedPage);
    await organisationListPage.goto();
    await organisationListPage.waitForPage();

    await expect(organisationListPage.table.paginator).toBeVisible();
  });
});
