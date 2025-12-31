import { test, expect } from '../fixtures/auth.fixture';
import { HomePage } from '../pages/home.page';

test.describe('Home Page', () => {
  test('should display home page heading', async ({ authenticatedPage }) => {
    const homePage = new HomePage(authenticatedPage);
    await homePage.goto();
    await homePage.waitForHomePage();

    await expect(homePage.heading).toHaveText('home works!');
  });

  test('should display throttled counter', async ({ authenticatedPage }) => {
    const homePage = new HomePage(authenticatedPage);
    await homePage.goto();
    await homePage.waitForHomePage();

    await expect(homePage.throttledCounter).toContainText('Throttled counter:');
  });

  test('should increment counter on throttled button click', async ({
    authenticatedPage,
  }) => {
    const homePage = new HomePage(authenticatedPage);
    await homePage.goto();
    await homePage.waitForHomePage();

    const initialCount = await homePage.getThrottledCounter();
    await homePage.clickThrottledButton();

    // Wait for potential state update
    await authenticatedPage.waitForTimeout(100);

    const newCount = await homePage.getThrottledCounter();
    expect(newCount).toBe(initialCount + 1);
  });

  test('should throttle rapid button clicks', async ({ authenticatedPage }) => {
    const homePage = new HomePage(authenticatedPage);
    await homePage.goto();
    await homePage.waitForHomePage();

    const initialCount = await homePage.getThrottledCounter();

    // Click rapidly multiple times
    await homePage.clickThrottledButton();
    await homePage.clickThrottledButton();
    await homePage.clickThrottledButton();

    await authenticatedPage.waitForTimeout(100);

    // Should only increment once due to throttling
    const newCount = await homePage.getThrottledCounter();
    expect(newCount).toBe(initialCount + 1);
  });
});
