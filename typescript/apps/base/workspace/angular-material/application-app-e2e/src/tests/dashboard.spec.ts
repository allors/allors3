import { test, expect } from '../fixtures/auth.fixture';
import { DashboardPage } from '../pages/dashboard.page';
import { MainPage } from '../pages/main.page';

test.describe('Dashboard Page', () => {
  test('should display dashboard page', async ({ authenticatedPage }) => {
    const dashboardPage = new DashboardPage(authenticatedPage);
    await dashboardPage.goto();
    await dashboardPage.waitForDashboardPage();

    // Main sidenav container should be visible
    await expect(
      authenticatedPage.locator('mat-sidenav-container')
    ).toBeVisible();
  });

  test('should have sidenav toggle visible', async ({ authenticatedPage }) => {
    const dashboardPage = new DashboardPage(authenticatedPage);
    await dashboardPage.goto();
    await dashboardPage.waitForDashboardPage();

    // The toggle is in the dashboard toolbar
    await expect(dashboardPage.sidenavToggle).toBeVisible();
  });

  test('should display toolbar with home button', async ({
    authenticatedPage,
  }) => {
    const mainPage = new MainPage(authenticatedPage);
    await authenticatedPage.goto('/dashboard');
    await authenticatedPage
      .locator('mat-sidenav-container')
      .waitFor({ state: 'visible' });

    await expect(mainPage.toolbar).toBeVisible();
    await expect(mainPage.homeButton).toBeVisible();
  });
});
