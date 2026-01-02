import { Page, Locator } from '@playwright/test';
import { MaterialSidenavComponent } from '../components/material-sidenav.component';

export class MainPage {
  readonly page: Page;
  readonly sidenav: MaterialSidenavComponent;
  readonly toolbar: Locator;
  readonly homeButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.sidenav = new MaterialSidenavComponent(page);
    this.toolbar = page.locator('mat-toolbar[color="primary"]');
    this.homeButton = this.toolbar.locator('button', {
      hasText: 'Allors Applications',
    });
  }

  async goHome(): Promise<void> {
    await this.homeButton.click();
  }

  async navigateViaMenu(menuPath: string[]): Promise<void> {
    await this.sidenav.open();
    if (menuPath.length === 1) {
      await this.sidenav.clickMenuItem(menuPath[0]);
    } else {
      await this.sidenav.clickChildMenuItem(menuPath[0], menuPath[1]);
    }
  }
}
