import { test, expect } from '../fixtures/auth.fixture';
import { MainPage } from '../pages/common/main.page';
import { PersonListPage } from '../pages/contacts/person-list.page';
import { OrganisationListPage } from '../pages/contacts/organisation-list.page';
import { CommunicationEventListPage } from '../pages/contacts/communication-event-list.page';

test.describe('Navigation', () => {
  test('should display sidenav with menu items', async ({ authenticatedPage }) => {
    const mainPage = new MainPage(authenticatedPage);
    await authenticatedPage.goto('/');
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
    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });

    await expect(mainPage.sidenav.toggleButton).toBeVisible();
  });

  // Contacts menu
  test('should navigate to People via Contacts menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);
    const personListPage = new PersonListPage(authenticatedPage);

    await authenticatedPage.goto('/');
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

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await authenticatedPage.waitForTimeout(500);
    await mainPage.navigateViaMenu(['Contacts', 'Companies']);

    await organisationListPage.waitForPage();
    await expect(authenticatedPage).toHaveURL(/.*\/contacts\/organisations/);
  });

  test('should navigate to Communication Events via Contacts menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);
    const commEventListPage = new CommunicationEventListPage(authenticatedPage);

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['Contacts', 'CommunicationEvents']);

    await commEventListPage.waitForPage();
    await expect(authenticatedPage).toHaveURL(/.*\/contacts\/communicationevents/);
  });

  // Sales menu
  test('should navigate to Requests for Quote via Sales menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['Sales', 'RequestsForQuote']);

    await expect(authenticatedPage).toHaveURL(/.*\/sales\/requestsforquote/);
  });

  test('should navigate to Product Quotes via Sales menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['Sales', 'ProductQuotes']);

    await expect(authenticatedPage).toHaveURL(/.*\/sales\/productquotes/);
  });

  test('should navigate to Sales Orders via Sales menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['Sales', 'SalesOrders']);

    await expect(authenticatedPage).toHaveURL(/.*\/sales\/salesorders/);
  });

  test('should navigate to Sales Invoices via Sales menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['Sales', 'SalesInvoices']);

    await expect(authenticatedPage).toHaveURL(/.*\/sales\/salesinvoices/);
  });

  // Products menu
  test('should navigate to Goods via Products menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['Products', 'Goods']);

    await expect(authenticatedPage).toHaveURL(/.*\/products\/goods/);
  });

  test('should navigate to Parts via Products menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['Products', 'Parts']);

    await expect(authenticatedPage).toHaveURL(/.*\/products\/parts/);
  });

  // Purchasing menu
  test('should navigate to Purchase Orders via Purchasing menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['Purchasing', 'PurchaseOrders']);

    await expect(authenticatedPage).toHaveURL(/.*\/purchasing\/purchaseorders/);
  });

  test('should navigate to Purchase Invoices via Purchasing menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['Purchasing', 'PurchaseInvoices']);

    await expect(authenticatedPage).toHaveURL(/.*\/purchasing\/purchaseinvoices/);
  });

  // Shipment menu
  test('should navigate to Shipments via Shipments menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['Shipments', 'Shipments']);

    await expect(authenticatedPage).toHaveURL(/.*\/shipment\/shipments/);
  });

  // Work Efforts menu
  test('should navigate to Work Requirements via WorkEfforts menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['WorkEfforts', 'WorkRequirements']);

    await expect(authenticatedPage).toHaveURL(/.*\/workefforts\/workrequirements/);
  });

  test('should navigate to Work Efforts via WorkEfforts menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['WorkEfforts', 'WorkEfforts']);

    await expect(authenticatedPage).toHaveURL(/.*\/workefforts\/workefforts/);
  });

  // Accounting menu
  test('should navigate to Exchange Rates via Accounting menu', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });
    await mainPage.navigateViaMenu(['Accounting', 'ExchangeRates']);

    await expect(authenticatedPage).toHaveURL(/.*\/accounting\/exchangerates/);
  });

  // Toolbar navigation
  test('should navigate home via toolbar button', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);

    await authenticatedPage.goto('/contacts/people');
    await mainPage.goHome();

    await authenticatedPage.locator('mat-sidenav-container').waitFor({ state: 'visible' });
  });

  test('should maintain authentication during navigation', async ({
    authenticatedPage,
  }) => {
    await authenticatedPage.goto('/');
    await authenticatedPage.goto('/contacts/people');
    await authenticatedPage.goto('/contacts/organisations');
    await authenticatedPage.goto('/sales/salesorders');

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

    // Direct navigation to sales orders
    await authenticatedPage.goto('/sales/salesorders');
    await expect(authenticatedPage).toHaveURL(/.*\/sales\/salesorders/);

    // Direct navigation to products
    await authenticatedPage.goto('/products/goods');
    await expect(authenticatedPage).toHaveURL(/.*\/products\/goods/);
  });
});
