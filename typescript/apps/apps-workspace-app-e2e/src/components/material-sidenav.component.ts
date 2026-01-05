import { Page, Locator } from '@playwright/test';

export class MaterialSidenavComponent {
  readonly page: Page;
  readonly container: Locator;
  readonly drawer: Locator;
  readonly toggleButton: Locator;
  readonly menu: Locator;

  constructor(page: Page) {
    this.page = page;
    this.container = page.locator('mat-sidenav-container');
    this.drawer = page.locator('mat-sidenav').first();
    this.toggleButton = page.locator('a-mat-sidenavtoggle button').first();
    this.menu = page.locator('a-mat-sidemenu');
  }

  async isOpen(): Promise<boolean> {
    // Check if menu is visible (regardless of sidenav state)
    return await this.menu.isVisible();
  }

  async toggle(): Promise<void> {
    await this.toggleButton.click();
    await this.page.waitForTimeout(300);
  }

  async open(): Promise<void> {
    // Check if menu is already visible
    const isVisible = await this.menu.isVisible();
    if (!isVisible) {
      // Toggle sidenav to open it
      await this.toggle();
      await this.menu.waitFor({ state: 'visible', timeout: 5000 });
    }
  }

  async close(): Promise<void> {
    // In side mode, clicking toggle doesn't really close it
    // This is a no-op for side mode sidenavs
  }

  async getMenuItems(): Promise<string[]> {
    const items = this.menu.locator('mat-list-item');
    const count = await items.count();
    const titles: string[] = [];
    for (let i = 0; i < count; i++) {
      const text = await items.nth(i).textContent();
      if (text) titles.push(text.trim());
    }
    return titles;
  }

  async clickMenuItem(title: string): Promise<void> {
    const item = this.menu.locator('mat-list-item', { hasText: title }).first();
    await item.click();
  }

  async expandMenuItem(title: string): Promise<void> {
    const item = this.menu.locator('mat-list-item', { hasText: title }).first();
    const isExpanded = (await item.getAttribute('class'))?.includes('expanded');
    if (!isExpanded) {
      await item.click();
      await this.page.waitForTimeout(200);
    }
  }

  async clickChildMenuItem(
    parentTitle: string,
    childTitle: string
  ): Promise<void> {
    await this.expandMenuItem(parentTitle);
    const childItem = this.menu
      .locator('mat-list-item.expanded', {
        hasText: childTitle,
      })
      .first();
    await childItem.click();
  }

  async navigateToRoute(link: string): Promise<void> {
    const menuItem = this.menu.locator(`a[href="${link}"]`);
    await menuItem.click();
  }
}
