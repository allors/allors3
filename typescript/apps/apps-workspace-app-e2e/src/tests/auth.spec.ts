import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/common/login.page';
import { DashboardPage } from '../pages/common/dashboard.page';
import { ApiClient } from '../support/api-client';

test.describe('Authentication', () => {
  let loginPage: LoginPage;
  let dashboardPage: DashboardPage;
  let apiClient: ApiClient;

  test.beforeEach(async ({ page }) => {
    apiClient = new ApiClient();
    await apiClient.setup('full');
    loginPage = new LoginPage(page);
    dashboardPage = new DashboardPage(page);
  });

  test('should display login form with Material card', async ({ page }) => {
    await loginPage.goto();
    await expect(loginPage.card).toBeVisible();
    await expect(loginPage.heading).toHaveText('Login');
    await expect(loginPage.usernameInput).toBeVisible();
    await expect(loginPage.passwordInput).toBeVisible();
    await expect(loginPage.signInButton).toBeVisible();
  });

  test('should login successfully with valid credentials', async ({ page }) => {
    await loginPage.goto();
    await loginPage.login('jane@example.com', '');
    await dashboardPage.waitForDashboardPage();
  });

  test('should show error for invalid credentials', async ({ page }) => {
    await loginPage.goto();

    page.on('dialog', async (dialog) => {
      expect(dialog.message()).toContain('Could not log in');
      await dialog.accept();
    });

    await loginPage.login('invalid@example.com', 'wrongpassword');
    await expect(page).toHaveURL(/.*login/);
  });

  test('should redirect unauthenticated users to login', async ({ page }) => {
    await page.goto('/contacts/people');
    await loginPage.waitForLoginPage();
    await expect(page).toHaveURL(/.*login/);
  });

  test('should redirect to home after successful login', async ({
    page,
  }) => {
    await loginPage.goto();
    await loginPage.login('jane@example.com', '');
    await dashboardPage.waitForDashboardPage();
  });

  test('should persist session across navigation', async ({ page }) => {
    await loginPage.goto();
    await loginPage.login('jane@example.com', '');
    await dashboardPage.waitForDashboardPage();

    // Navigate to a protected route
    await page.goto('/contacts/people');
    await expect(page).not.toHaveURL(/.*login/);

    // Navigate back to home
    await page.goto('/');
    await expect(page).not.toHaveURL(/.*login/);
  });
});
