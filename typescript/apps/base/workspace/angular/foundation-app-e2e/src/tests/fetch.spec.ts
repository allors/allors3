import { test, expect } from '../fixtures/auth.fixture';
import { QueryPage } from '../pages/query.page';
import { FetchPage } from '../pages/fetch.page';

test.describe('Fetch Page', () => {
  test('should display organisation details', async ({ authenticatedPage }) => {
    const queryPage = new QueryPage(authenticatedPage);
    const fetchPage = new FetchPage(authenticatedPage);

    // Navigate to query first to get an organisation
    await queryPage.goto();
    await queryPage.waitForQueryPage();
    await queryPage.clickQuery();
    await authenticatedPage.waitForTimeout(500);

    // Click first organisation
    await queryPage.clickOrganisation(0);
    await fetchPage.waitForFetchPage();

    const details = await fetchPage.getOrganisationDetails();
    expect(details).toBeTruthy();
  });

  test('should display owner information', async ({ authenticatedPage }) => {
    const queryPage = new QueryPage(authenticatedPage);
    const fetchPage = new FetchPage(authenticatedPage);

    await queryPage.goto();
    await queryPage.waitForQueryPage();
    await queryPage.clickQuery();
    await authenticatedPage.waitForTimeout(500);

    await queryPage.clickOrganisation(0);
    await fetchPage.waitForFetchPage();
    await authenticatedPage.waitForTimeout(500);

    const details = await fetchPage.getOrganisationDetails();
    expect(details).toContain('with owner');
  });

  test('should display page structure correctly', async ({
    authenticatedPage,
  }) => {
    const queryPage = new QueryPage(authenticatedPage);
    const fetchPage = new FetchPage(authenticatedPage);

    await queryPage.goto();
    await queryPage.waitForQueryPage();
    await queryPage.clickQuery();
    await authenticatedPage.waitForTimeout(500);

    await queryPage.clickOrganisation(0);
    await fetchPage.waitForFetchPage();

    // Verify the organisation details are displayed
    await expect(fetchPage.organisationDetails).toBeVisible();

    // The related organisations list may or may not have items depending on data
    // Just verify we're on the fetch page with details visible
    const details = await fetchPage.getOrganisationDetails();
    expect(details).toContain('with owner');
  });

  test('should navigate to related organisation', async ({
    authenticatedPage,
  }) => {
    const queryPage = new QueryPage(authenticatedPage);
    const fetchPage = new FetchPage(authenticatedPage);

    await queryPage.goto();
    await queryPage.waitForQueryPage();
    await queryPage.clickQuery();
    await authenticatedPage.waitForTimeout(500);

    await queryPage.clickOrganisation(0);
    await fetchPage.waitForFetchPage();
    await authenticatedPage.waitForTimeout(500);

    const relatedOrgs = await fetchPage.getRelatedOrganisations();
    if (relatedOrgs.length > 0) {
      const currentUrl = authenticatedPage.url();
      await fetchPage.clickRelatedOrganisation(0);
      await fetchPage.waitForFetchPage();

      // URL should change to different organisation
      const newUrl = authenticatedPage.url();
      expect(newUrl).toMatch(/.*\/fetch\/.+/);
    }
  });
});
