import { test, expect } from '../fixtures/auth.fixture';
import { MainPage } from '../pages/main.page';
import { DashboardPage } from '../pages/dashboard.page';
import { PersonListPage } from '../pages/person-list.page';
import { OrganisationListPage } from '../pages/organisation-list.page';
import { CountryListPage } from '../pages/country-list.page';

test.describe('Navigation', () => {
  test('should display sidenav with menu items', async ({ authenticatedPage }) => {
    const mainPage = new MainPage(authenticatedPage);
    await authenticatedPage.goto('/dashboard');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });

    await mainPage.sidenav.open();
    const menuItems = await mainPage.sidenav.getMenuItems();

    expect(menuItems.length).toBeGreaterThan(0);
    expect(menuItems.some((item) => item.includes('Home'))).toBeTruthy();
  });

  test('should display sidenav toggle button', async ({ authenticatedPage }) => {
    const mainPage = new MainPage(authenticatedPage);
    await authenticatedPage.goto('/dashboard');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });

    await expect(mainPage.sidenav.toggleButton).toBeVisible();
  });

  test('should navigate to People via Contacts menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);
    const personListPage = new PersonListPage(authenticatedPage);

    await authenticatedPage.goto('/dashboard');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['Contacts', 'People']);

    await personListPage.waitForPage();
    await expect(authenticatedPage).toHaveURL(/.*\/contacts\/people/);
  });

  test('should navigate to Organisations via Contacts menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);
    const organisationListPage = new OrganisationListPage(authenticatedPage);

    await authenticatedPage.goto('/dashboard');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    // Wait a bit for Angular to fully initialize
    await authenticatedPage.waitForTimeout(500);
    // Menu item is labeled "Companies" in the UI
    await mainPage.navigateViaMenu(['Contacts', 'Companies']);

    await organisationListPage.waitForPage();
    await expect(authenticatedPage).toHaveURL(/.*\/contacts\/organisations/);
  });

  test('should navigate to Countries via Contacts menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);
    const countryListPage = new CountryListPage(authenticatedPage);

    await authenticatedPage.goto('/dashboard');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['Contacts', 'Countries']);

    await countryListPage.waitForPage();
    await expect(authenticatedPage).toHaveURL(/.*\/contacts\/countries/);
  });

  test('should navigate home via toolbar button', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/contacts/people');
    await mainPage.goHome();

    await expect(authenticatedPage).toHaveURL(/.*dashboard/);
  });

  test('should maintain authentication during navigation', async ({
    authenticatedPage,
  }) => {
    await authenticatedPage.goto('/dashboard');
    await authenticatedPage.goto('/contacts/people');
    await authenticatedPage.goto('/contacts/organisations');
    await authenticatedPage.goto('/contacts/countries');

    // Should not redirect to login
    await expect(authenticatedPage).not.toHaveURL(/.*login/);
  });

  test('should navigate via direct URL to protected routes', async ({
    authenticatedPage,
  }) => {
    // Direct navigation to people
    await authenticatedPage.goto('/contacts/people');
    await expect(authenticatedPage).toHaveURL(/.*\/contacts\/people/);

    // Direct navigation to organisations
    await authenticatedPage.goto('/contacts/organisations');
    await expect(authenticatedPage).toHaveURL(/.*\/contacts\/organisations/);

    // Direct navigation to countries
    await authenticatedPage.goto('/contacts/countries');
    await expect(authenticatedPage).toHaveURL(/.*\/contacts\/countries/);
  });
});
