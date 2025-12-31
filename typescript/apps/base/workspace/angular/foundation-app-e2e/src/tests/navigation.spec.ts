import { test, expect } from '../fixtures/auth.fixture';
import { HomePage } from '../pages/home.page';
import { QueryPage } from '../pages/query.page';
import { FetchPage } from '../pages/fetch.page';

test.describe('Navigation Flow', () => {
  test('should navigate from home to query to fetch and back', async ({
    authenticatedPage,
  }) => {
    const homePage = new HomePage(authenticatedPage);
    const queryPage = new QueryPage(authenticatedPage);
    const fetchPage = new FetchPage(authenticatedPage);

    // Start at home
    await homePage.goto();
    await homePage.waitForHomePage();
    await expect(homePage.heading).toHaveText('home works!');

    // Navigate to query
    await authenticatedPage.goto('/query');
    await queryPage.waitForQueryPage();

    // Query and click first org
    await queryPage.clickQuery();
    await authenticatedPage.waitForTimeout(500);
    await queryPage.clickOrganisation(0);

    // Verify on fetch page
    await fetchPage.waitForFetchPage();
    await expect(authenticatedPage).toHaveURL(/.*\/fetch\/.+/);

    // Navigate back
    await authenticatedPage.goBack();
    await expect(authenticatedPage).toHaveURL(/.*\/query/);
  });

  test('should maintain authentication across navigation', async ({
    authenticatedPage,
  }) => {
    await authenticatedPage.goto('/');
    await expect(authenticatedPage).toHaveURL('/');

    await authenticatedPage.goto('/query');
    await expect(authenticatedPage).toHaveURL(/.*\/query/);

    // Should not redirect to login
    await expect(authenticatedPage).not.toHaveURL(/.*\/login/);
  });

  test('should navigate using header links', async ({ authenticatedPage }) => {
    const homePage = new HomePage(authenticatedPage);

    await homePage.goto();
    await homePage.waitForHomePage();

    // Click Query link in the header navigation (first ul > li > a)
    const queryLink = authenticatedPage.locator('header + ul a', {
      hasText: 'Query',
    });
    await queryLink.click();
    await expect(authenticatedPage).toHaveURL(/.*\/query/);

    // Click Home link in the header navigation
    const homeLink = authenticatedPage.locator('header + ul a', {
      hasText: 'Home',
    });
    await homeLink.click();
    await expect(authenticatedPage).toHaveURL('/');
  });

  test('should handle direct URL navigation to protected routes', async ({
    authenticatedPage,
  }) => {
    // Navigate directly to query page
    await authenticatedPage.goto('/query');
    await expect(authenticatedPage).toHaveURL(/.*\/query/);

    // Should still be authenticated
    await expect(authenticatedPage).not.toHaveURL(/.*\/login/);
  });
});
