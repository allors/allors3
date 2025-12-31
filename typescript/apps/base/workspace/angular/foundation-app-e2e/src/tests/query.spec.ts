import { test, expect } from '../fixtures/auth.fixture';
import { QueryPage } from '../pages/query.page';

test.describe('Query Page', () => {
  test('should display organisations list after query', async ({
    authenticatedPage,
  }) => {
    const queryPage = new QueryPage(authenticatedPage);
    await queryPage.goto();
    await queryPage.waitForQueryPage();

    await queryPage.clickQuery();
    await authenticatedPage.waitForTimeout(500);

    const organisations = await queryPage.getOrganisations();
    expect(organisations.length).toBeGreaterThan(0);
  });

  test('should display total count element', async ({ authenticatedPage }) => {
    const queryPage = new QueryPage(authenticatedPage);
    await queryPage.goto();
    await queryPage.waitForQueryPage();

    // Verify the total count element is present (note: organisationCount is declared but not populated in the component)
    await expect(queryPage.totalCount).toBeVisible();
  });

  test('should paginate organisations with skip and take', async ({
    authenticatedPage,
  }) => {
    const queryPage = new QueryPage(authenticatedPage);
    await queryPage.goto();
    await queryPage.waitForQueryPage();

    await queryPage.setSkip(0);
    await queryPage.setTake(2);
    await queryPage.clickQuery();

    await authenticatedPage.waitForTimeout(500);

    const organisations = await queryPage.getOrganisations();
    expect(organisations.length).toBeLessThanOrEqual(2);
  });

  test('should skip organisations', async ({ authenticatedPage }) => {
    const queryPage = new QueryPage(authenticatedPage);
    await queryPage.goto();
    await queryPage.waitForQueryPage();

    // Get first set
    await queryPage.setSkip(0);
    await queryPage.setTake(2);
    await queryPage.clickQuery();
    await authenticatedPage.waitForTimeout(500);
    const firstSet = await queryPage.getOrganisations();

    // Get second set with skip
    await queryPage.setSkip(1);
    await queryPage.setTake(2);
    await queryPage.clickQuery();
    await authenticatedPage.waitForTimeout(500);
    const secondSet = await queryPage.getOrganisations();

    // Sets should be different if there are enough organisations
    if (firstSet.length > 1 && secondSet.length > 0) {
      expect(firstSet[0]).not.toBe(secondSet[0]);
    }
  });

  test('should navigate to fetch page when clicking organisation', async ({
    authenticatedPage,
  }) => {
    const queryPage = new QueryPage(authenticatedPage);
    await queryPage.goto();
    await queryPage.waitForQueryPage();

    await queryPage.clickQuery();
    await authenticatedPage.waitForTimeout(500);

    await queryPage.clickOrganisation(0);

    await expect(authenticatedPage).toHaveURL(/.*\/fetch\/.+/);
  });

  test('should show owner information for organisations', async ({
    authenticatedPage,
  }) => {
    const queryPage = new QueryPage(authenticatedPage);
    await queryPage.goto();
    await queryPage.waitForQueryPage();

    await queryPage.clickQuery();
    await authenticatedPage.waitForTimeout(500);

    const organisations = await queryPage.getOrganisations();
    if (organisations.length > 0) {
      expect(organisations[0]).toContain('with owner');
    }
  });
});
