import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/login.page';
import { HomePage } from '../pages/home.page';
import { ApiClient } from '../support/api-client';

test.describe('Login Page', () => {
  let loginPage: LoginPage;
  let homePage: HomePage;
  let apiClient: ApiClient;

  test.beforeEach(async ({ page }) => {
    apiClient = new ApiClient();
    await apiClient.setup('full');

    loginPage = new LoginPage(page);
    homePage = new HomePage(page);
  });

  test('should display login form', async ({ page }) => {
    await loginPage.goto();

    await expect(loginPage.heading).toHaveText('Login');
    await expect(loginPage.usernameInput).toBeVisible();
    await expect(loginPage.passwordInput).toBeVisible();
    await expect(loginPage.signInButton).toBeVisible();
  });

  test('should login successfully with valid credentials', async ({ page }) => {
    await loginPage.goto();
    await loginPage.login('jane@example.com', '');

    await homePage.waitForHomePage();
    await expect(homePage.heading).toHaveText('home works!');
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
    await page.goto('/');
    await loginPage.waitForLoginPage();
  });

  test('should redirect to home after successful login', async ({ page }) => {
    await loginPage.goto();
    await loginPage.login('jane@example.com', '');

    await expect(page).toHaveURL('/');
  });
});
