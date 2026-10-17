import AxeBuilder from '@axe-core/playwright';
import { expect, test, type Locator, type Page } from '@playwright/test';

async function tabTo(page: Page, target: Locator) {
  for (let attempt = 0; attempt < 60; attempt++) {
    if (await target.evaluate((element) => element === document.activeElement)) return;
    await page.keyboard.press('Tab');
  }
  await expect(target, 'The control must be reachable by keyboard').toBeFocused();
}

async function openMobileMenu(page: Page) {
  const menu = page.getByRole('button', { name: 'Menu', exact: true });
  if (await menu.isVisible()) {
    await tabTo(page, menu);
    await page.keyboard.press('Enter');
    await expect(page.locator('#starlight__sidebar:popover-open')).toBeVisible();
  }
}

async function expectNoPageOverflow(page: Page) {
  const dimensions = await page.evaluate(() => ({
    viewport: document.documentElement.clientWidth,
    document: document.documentElement.scrollWidth,
    body: document.body.scrollWidth,
  }));
  expect(dimensions.document, 'The page must fit the viewport; tables may scroll internally')
    .toBeLessThanOrEqual(dimensions.viewport + 1);
  expect(dimensions.body).toBeLessThanOrEqual(dimensions.viewport + 1);
}

test('skip link and sidebar navigation work from the keyboard', async ({ page }) => {
  await page.goto('./');
  await page.keyboard.press('Tab');
  const skip = page.getByRole('link', { name: 'Skip to content', exact: true });
  await expect(skip).toBeFocused();
  await expect(skip).toBeVisible();
  const target = await skip.getAttribute('href');
  await page.keyboard.press('Enter');
  await expect.poll(() => new URL(page.url()).hash).toBe(target);
  await page.keyboard.press('Tab');
  await expect(page.getByRole('link', { name: 'Markdown', exact: true })).toBeFocused();

  await page.goto('./');
  await openMobileMenu(page);
  const domains = page.locator('#starlight__sidebar').getByRole('link', { name: 'Domains', exact: true });
  await tabTo(page, domains);
  await page.keyboard.press('Enter');
  await expect(page.getByRole('heading', { level: 1, name: 'Domains', exact: true })).toBeVisible();
  await expect(page.locator('#starlight__sidebar:popover-open')).toHaveCount(0);
  await expectNoPageOverflow(page);
});

test('keyboard search finds the workspace editing baseline', async ({ page }) => {
  await page.goto('./');
  const searchButton = page.getByRole('button', { name: 'Search', exact: true });
  await expect(searchButton).toBeEnabled();
  await tabTo(page, searchButton);
  await page.keyboard.press('Enter');
  const dialog = page.getByRole('dialog', { name: 'Search', exact: true });
  await expect(dialog).toBeVisible();
  const input = dialog.getByRole('textbox', { name: 'Search', exact: true });
  await expect(input).toBeVisible();
  await tabTo(page, input);
  await page.keyboard.type('editing baseline');
  const result = dialog.getByRole('link', { name: 'Workspace session state', exact: true });
  await expect(result).toBeVisible();
  await tabTo(page, result);
  await page.keyboard.press('Enter');
  await expect(page.getByRole('heading', { level: 1, name: 'Workspace session state', exact: true })).toBeVisible();
  await expect(dialog).not.toBeVisible();
  await expectNoPageOverflow(page);
});

test('Mermaid renders labelled diagrams and responds to theme changes', async ({ page }, testInfo) => {
  await page.goto('domains/');
  const diagram = page.locator('pre.mermaid svg');
  await expect(diagram).toBeVisible();
  await expect(diagram.locator('title')).toHaveText('A concrete domain selects functional domains and a plug-in');
  await expect(diagram.locator('desc')).toContainText('Custom extends Sales, Stock and Identity.');
  await expect(diagram).toHaveAccessibleName('A concrete domain selects functional domains and a plug-in');
  await expect(diagram.locator('.node')).toHaveCount(5);
  const labelSize = await diagram.evaluate((element) => {
    const svg = element as SVGSVGElement;
    const label = svg.querySelector('.nodeLabel')!;
    return parseFloat(getComputedStyle(label).fontSize) * svg.clientWidth / svg.viewBox.baseVal.width;
  });
  expect(labelSize, 'Rendered diagram labels must remain readable at narrow widths').toBeGreaterThanOrEqual(12);
  if (page.viewportSize()!.width < 800) {
    const container = page.locator('pre.mermaid');
    await tabTo(page, container);
    await page.keyboard.press('ArrowRight');
    await expect.poll(() => container.evaluate((element) => element.scrollLeft)).toBeGreaterThan(0);
  }
  await expectNoPageOverflow(page);
  await page.evaluate(() => window.scrollTo(0, 0));
  await expect(page.getByRole('heading', { level: 1, name: 'Domains', exact: true })).toBeInViewport();
  await page.screenshot({ path: testInfo.outputPath('domains-light.png') });
  await page.locator('pre.mermaid').scrollIntoViewIfNeeded();
  await expect(page.locator('pre.mermaid')).toBeInViewport({ ratio: 1 });
  await page.locator('pre.mermaid').screenshot({ path: testInfo.outputPath('domain-diagram-light.png') });

  const lightId = await diagram.getAttribute('id');
  const lightFill = await diagram.locator('.node rect').first().evaluate((element) => getComputedStyle(element).fill);
  await openMobileMenu(page);
  const theme = page.getByRole('combobox', { name: 'Select theme', exact: true }).filter({ visible: true });
  await theme.selectOption('dark');
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  await expect.poll(() => diagram.getAttribute('id')).not.toBe(lightId);
  await expect(diagram).toBeVisible();
  await expect.poll(() => diagram.locator('.node rect').first().evaluate((element) => getComputedStyle(element).fill))
    .not.toBe(lightFill);
  await expect(diagram).toHaveAccessibleName('A concrete domain selects functional domains and a plug-in');
  await page.keyboard.press('Escape');
  await expectNoPageOverflow(page);
  await page.evaluate(() => window.scrollTo(0, 0));
  await expect(page.getByRole('heading', { level: 1, name: 'Domains', exact: true })).toBeInViewport();
  await page.screenshot({ path: testInfo.outputPath('domains-dark.png') });
  await page.locator('pre.mermaid').scrollIntoViewIfNeeded();
  await expect(page.locator('pre.mermaid')).toBeInViewport({ ratio: 1 });
  await page.locator('pre.mermaid').screenshot({ path: testInfo.outputPath('domain-diagram-dark.png') });

  const accessibility = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa']).analyze();
  expect(accessibility.violations).toEqual([]);
});

for (const [slug, title] of [
  ['', 'Documentation'],
  ['domains/', 'Domains'],
  ['internals/workspace-session-state/', 'Workspace session state'],
]) {
  test(`${title} has an accessible layout`, async ({ page }, testInfo) => {
    const errors: string[] = [];
    page.on('pageerror', (error) => errors.push(error.message));
    await page.goto(slug || './');
    await expect(page.getByRole('heading', { level: 1, name: title, exact: true })).toBeVisible();
    if (slug === 'domains/') await expect(page.locator('pre.mermaid svg')).toBeVisible();
    await expectNoPageOverflow(page);
    const accessibility = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa']).analyze();
    expect(accessibility.violations).toEqual([]);
    expect(errors, 'Page scripts must run without uncaught errors').toEqual([]);
    if (slug === 'internals/workspace-session-state/' && page.viewportSize()!.width < 800) {
      const tables = page.locator('.sl-markdown-content table');
      const overflowing = await tables.evaluateAll((elements) => elements.findIndex((element) =>
        element.scrollWidth > element.clientWidth));
      expect(overflowing, 'The narrow layout should exercise a horizontally scrolling table').toBeGreaterThanOrEqual(0);
      const table = tables.nth(overflowing);
      await tabTo(page, table);
      await page.keyboard.press('ArrowRight');
      await expect.poll(() => table.evaluate((element) => element.scrollLeft)).toBeGreaterThan(0);
      await page.evaluate(() => window.scrollTo(0, 0));
    }
    if (slug) {
      const screenshot = slug.replaceAll('/', '-');
      await page.screenshot({ path: testInfo.outputPath(`${screenshot}.png`) });
      await page.screenshot({ path: testInfo.outputPath(`${screenshot}-full.png`), fullPage: true });
    }
  });
}
