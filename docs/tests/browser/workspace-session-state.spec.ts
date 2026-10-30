import AxeBuilder from '@axe-core/playwright';
import { expect, test, type Locator, type Page } from '@playwright/test';

const route = 'internals/workspace-session-state/';
const scenarios = [
  { id: 'remote-refresh', steps: ['start', 'local-edit', 'remote-push', 'remote-pull', 'push', 'reset', 'recovery-pull'] },
  { id: 'disjoint', steps: ['start', 'local-edit', 'remote-push', 'remote-pull', 'editing-pull', 'push', 'recovery-pull'] },
  { id: 'string-conflict', steps: ['start', 'local-edit', 'remote-push', 'remote-pull', 'editing-pull', 'reset', 'recovery-pull'] },
  { id: 'to-one-conflict', steps: ['start', 'local-edit', 'remote-push', 'remote-pull', 'editing-pull', 'reset', 'recovery-pull'] },
  { id: 'to-many-conflict', steps: ['start', 'local-edit', 'remote-push', 'remote-pull', 'editing-pull', 'reset', 'recovery-pull'] },
  { id: 'push-reset', steps: ['start', 'local-edit', 'push', 'reset', 'recovery-pull'] },
  { id: 'permissions', steps: ['start', 'local-edit', 'remote-push', 'remote-pull', 'permission-refresh', 'editing-pull', 'reset', 'recovery-pull'] },
  { id: 'final-selection', steps: ['start', 'first-pull', 'selection-changes', 'sync', 'cache-hit'] },
];

async function tabTo(page: Page, target: Locator) {
  for (let attempt = 0; attempt < 80; attempt++) {
    if (await target.evaluate((element) => element === document.activeElement)) return;
    await page.keyboard.press('Tab');
  }
  await expect(target, 'The walkthrough control must be reachable by keyboard').toBeFocused();
}

function currentFrame(page: Page) {
  return page.locator('[data-workspace-example] [data-live-frame] [data-frame]');
}

async function showStep(page: Page, scenario: string, step: string) {
  const example = page.locator('[data-workspace-example]');
  await example.getByLabel('Scenario', { exact: true }).selectOption(scenario);
  await example.getByLabel('Step', { exact: true }).selectOption(step);
  const frame = currentFrame(page);
  await expect(frame).toHaveAttribute('data-scenario-id', scenario);
  await expect(frame).toHaveAttribute('data-step-id', step);
  return frame;
}

async function expectValues(frame: Locator, role: string, values: Record<string, string>) {
  const table = frame.getByRole('table', { name: 'Values after this step', exact: true });
  const headers = await table.getByRole('columnheader').allTextContents();
  const row = table.getByRole('row').filter({ has: frame.page().getByRole('rowheader', { name: role, exact: true }) });
  for (const [column, value] of Object.entries(values)) {
    const index = headers.findIndex((header) => header.trim() === column);
    expect(index, `The ${column} column must exist`).toBeGreaterThan(0);
    await expect(row.getByRole('cell').nth(index - 1), `${role}: ${column}`).toHaveText(value);
  }
}

async function expectVersions(frame: Locator, versions: [string, string, string]) {
  for (const [index, owner] of ['Database', 'Connection', 'Retained record'].entries()) {
    const card = frame.locator('[data-records]').getByText(owner, { exact: true }).locator('..');
    await expect(card.getByText(versions[index], { exact: true })).toBeVisible();
  }
}

async function expectNoPageOverflow(page: Page) {
  const widths = await page.evaluate(() => ({
    viewport: document.documentElement.clientWidth,
    document: document.documentElement.scrollWidth,
    body: document.body.scrollWidth,
  }));
  expect(widths.document, 'Wide snapshots must scroll inside the example').toBeLessThanOrEqual(widths.viewport + 1);
  expect(widths.body).toBeLessThanOrEqual(widths.viewport + 1);
}

test('prepared scenarios support keyboard navigation, step selection and returning to previous values', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', (error) => errors.push(error.message));
  await page.goto(route);
  const example = page.locator('[data-workspace-example]');
  const scenario = example.getByLabel('Scenario', { exact: true });
  const step = example.getByLabel('Step', { exact: true });
  const previous = example.getByRole('button', { name: 'Previous step', exact: true });
  const next = example.getByRole('button', { name: 'Next step', exact: true });
  const status = example.getByRole('status');
  await expect(status).toHaveAttribute('aria-live', 'polite');
  await expect(scenario).toHaveValue('remote-refresh');
  await expect(previous).toBeDisabled();

  await tabTo(page, scenario);
  await page.keyboard.press('d');
  await page.keyboard.press('Tab');
  await expect(scenario).toHaveValue('disjoint');
  await tabTo(page, next);
  await page.keyboard.press('Enter');
  await expect(step).toHaveValue('local-edit');
  await expectValues(currentFrame(page), 'String', { 'Session read': '"local"', 'Diff original': '"baseline"' });
  await page.keyboard.press('Shift+Tab');
  await expect(previous).toBeFocused();
  await page.keyboard.press('Enter');
  await expect(step).toHaveValue('start');
  await expectValues(currentFrame(page), 'String', { 'Session read': '"baseline"' });

  for (const item of scenarios) {
    await scenario.selectOption(item.id);
    await expect(step).toHaveValue('start');
    const title = (await scenario.locator('option:checked').textContent())!.trim();
    await expect(step.locator('option')).toHaveCount(item.steps.length);
    for (let index = 0; index < item.steps.length; index++) {
      const frame = currentFrame(page);
      await expect(frame).toHaveCount(1);
      await expect(frame).toBeVisible();
      await expect(frame).toHaveAttribute('data-scenario-id', item.id);
      await expect(frame).toHaveAttribute('data-step-id', item.steps[index]);
      await expect(step).toHaveValue(item.steps[index]);
      const heading = (await frame.getByRole('heading', { level: 4 }).textContent())!.trim().replace(/^\d+\.\s*/, '');
      await expect(status).toContainText(title);
      await expect(status).toContainText(`Step ${index + 1} of ${item.steps.length}: ${heading}`);
      await expect(status).toContainText((await frame.locator('[data-outcome]').textContent())!.trim());
      await expect(previous).toBeEnabled({ enabled: index !== 0 });
      await expect(next).toBeEnabled({ enabled: index !== item.steps.length - 1 });
      await expect(frame.locator('[data-records]')).toContainText('Database');
      await expect(frame.locator('[data-records]')).toContainText('Connection');
      await expect(frame.locator('[data-records]')).toContainText('Retained record');
      await expectNoPageOverflow(page);
      if (index < item.steps.length - 1) await next.click();
    }
  }
  await scenario.selectOption('disjoint');
  await step.selectOption('editing-pull');
  await expectValues(currentFrame(page), 'Integer', { Retained: '2', 'Session read': '2' });
  await previous.click();
  await expect(step).toHaveValue('remote-pull');
  await expectValues(currentFrame(page), 'Integer', { Retained: '1', 'Session read': '1' });
  await scenario.selectOption('string-conflict');
  await expect(step).toHaveValue('start');
  await expect(previous).toBeDisabled();
  expect(errors, 'Changing walkthrough scenarios must not raise script errors').toEqual([]);
});

test('snapshots distinguish retained records, local edits, security and final results', async ({ page }, testInfo) => {
  await page.goto(route);
  let frame = await showStep(page, 'remote-refresh', 'remote-push');
  await expectVersions(frame, ['v2', 'v1', 'v1']);
  frame = await showStep(page, 'remote-refresh', 'remote-pull');
  await expectVersions(frame, ['v2', 'v2', 'v1']);
  await expectValues(frame, 'String', { Database: '"remote"', Connection: '"remote"', Retained: '"baseline"', 'Local edit': '"local"', 'Session read': '"local"', 'Diff original': '"baseline"' });
  await expectValues(frame, 'Integer', { Database: '2', Connection: '2', Retained: '1', 'Session read': '1' });
  frame = await showStep(page, 'remote-refresh', 'push');
  await expect(frame.locator('[data-outcome]')).toHaveText('Version error');
  await expect(frame).toContainText(/\.NET[^.]*returns/);
  await expect(frame).toContainText(/TypeScript[^.]*throws[^.]*ResultError/);
  frame = await showStep(page, 'remote-refresh', 'reset');
  await expectValues(frame, 'String', { Connection: '"remote"', Retained: '"baseline"', 'Session read': '"baseline"' });
  frame = await showStep(page, 'remote-refresh', 'recovery-pull');
  await expectValues(frame, 'String', { Retained: '"remote"', 'Session read': '"remote"' });

  frame = await showStep(page, 'disjoint', 'editing-pull');
  await expectVersions(frame, ['v2', 'v2', 'v2']);
  await expect(frame.locator('[data-outcome]')).toHaveText('Pull accepted');
  await expectValues(frame, 'Integer', { Database: '2', Connection: '2', Retained: '2', 'Session read': '2' });
  await expectValues(frame, 'String', { Retained: '"baseline"', 'Local edit': '"local"', 'Session read': '"local"', 'Diff original': '"baseline"' });
  frame = await showStep(page, 'disjoint', 'push');
  await expectVersions(frame, ['v3', 'v2', 'v2']);
  await expect(frame.locator('[data-outcome]')).toHaveText('Push succeeded');
  await expectValues(frame, 'Integer', { Database: '2', 'Session read': '2' });

  for (const id of ['string-conflict', 'to-one-conflict', 'to-many-conflict']) {
    frame = await showStep(page, id, 'editing-pull');
    await expectVersions(frame, ['v2', 'v2', 'v1']);
    await expect(frame.locator('[data-outcome]')).toHaveText('Merge error');
    await expectValues(frame, 'Integer', { Connection: '2', Retained: '1', 'Session read': '1' });
    await expectValues(frame, 'String', { Retained: '"baseline"', 'Session read': '"local"', 'Diff original': '"baseline"' });
    await expectValues(frame, 'To-one', { Retained: 'c1B', 'Session read': 'c1C', 'Diff original': 'c1B' });
    await expectValues(frame, 'To-many', { Retained: '{c1B}', 'Session read': '{c1B, c1C}', 'Diff original': '{c1B}' });
  }
  await expectValues(frame, 'To-many', { Connection: '{c1B, c1D}' });
  await expect(frame).not.toContainText('{c1B, c1C, c1D}');
  await expectNoPageOverflow(page);
  if (page.viewportSize()!.width < 800) {
    const table = frame.getByRole('table', { name: 'Values after this step', exact: true });
    await tabTo(page, table);
    await page.keyboard.press('ArrowRight');
    await expect.poll(() => table.evaluate((element) => element.scrollLeft)).toBeGreaterThan(0);
    await table.evaluate((element) => { element.scrollLeft = 0; });
    const controls = page.locator('[data-workspace-example] [data-controls]');
    await controls.scrollIntoViewIfNeeded();
    await page.evaluate(() => window.scrollBy(0, -160));
    await page.screenshot({ path: testInfo.outputPath('workspace-controls-narrow.png') });
    await frame.locator('[data-records]').screenshot({ path: testInfo.outputPath('workspace-records-narrow.png') });
  }
  await frame.scrollIntoViewIfNeeded();
  const screenshotStyle = 'header.header, mobile-starlight-toc { visibility: hidden !important; }';
  await frame.screenshot({ path: testInfo.outputPath('workspace-conflict-light.png'), style: screenshotStyle });
  let accessibility = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa']).analyze();
  expect(accessibility.violations).toEqual([]);

  const menu = page.getByRole('button', { name: 'Menu', exact: true });
  if (await menu.isVisible()) await menu.click();
  await page.getByRole('combobox', { name: 'Select theme', exact: true }).filter({ visible: true }).selectOption('dark');
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  await page.keyboard.press('Escape');
  await frame.scrollIntoViewIfNeeded();
  await frame.screenshot({ path: testInfo.outputPath('workspace-conflict-dark.png'), style: screenshotStyle });
  await expectNoPageOverflow(page);
  accessibility = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa']).analyze();
  expect(accessibility.violations).toEqual([]);

  frame = await showStep(page, 'push-reset', 'push');
  await expectValues(frame, 'String', { Database: '"X"', Retained: 'null', 'Session read': '"X"' });
  frame = await showStep(page, 'push-reset', 'reset');
  await expectValues(frame, 'String', { Database: '"X"', Retained: 'null', 'Session read': 'null' });
  frame = await showStep(page, 'push-reset', 'recovery-pull');
  await expectValues(frame, 'String', { Retained: '"X"', 'Session read': '"X"' });
  frame = await showStep(page, 'permissions', 'remote-pull');
  await expect(frame.locator('[data-retained-permission]')).toHaveText('allowed');
  frame = await showStep(page, 'permissions', 'permission-refresh');
  await expectVersions(frame, ['v2', 'v2', 'v1']);
  await expect(frame.locator('[data-outcome]')).toHaveText('Exact security record loaded');
  await expectValues(frame, 'Integer', { Connection: '2', Retained: '1', 'Session read': '1' });
  await expectValues(frame, 'String', { Connection: 'null', Retained: '"baseline"', 'Session read': '"local"' });
  await expect(frame.locator('.security-version')).toHaveText(['Grant G v2', 'Grant G v2', 'Grant G v1']);
  await expect(frame.locator('[data-retained-permission]')).toHaveText('allowed');
  frame = await showStep(page, 'permissions', 'editing-pull');
  await expect(frame.locator('[data-outcome]')).toHaveText('Merge error');
  await expect(frame.locator('[data-retained-permission]')).toHaveText('allowed');
  frame = await showStep(page, 'permissions', 'reset');
  await expectValues(frame, 'String', { 'Session read': '"baseline"' });
  await expect(frame.locator('[data-retained-permission]')).toHaveText('allowed');
  frame = await showStep(page, 'permissions', 'recovery-pull');
  await expect(frame.locator('[data-retained-permission]')).toHaveText('denied');
  await expectValues(frame, 'String', { Retained: 'null', 'Session read': 'null' });

  frame = await showStep(page, 'final-selection', 'first-pull');
  await expect(frame.locator('[data-selection]')).toContainText('Advertised to Sync: c1A.');
  let collection = frame.getByRole('table', { name: 'Named results for this query' }).getByRole('row').filter({ hasText: 'Ordered collection' });
  await expect(collection.getByRole('cell')).toHaveText(['c1A → c1E', 'c1A → c1E', 'c1A']);
  frame = await showStep(page, 'final-selection', 'sync');
  collection = frame.getByRole('table', { name: 'Named results for this query' }).getByRole('row').filter({ hasText: 'Ordered collection' });
  await expect(collection.getByRole('cell')).toHaveText(['c1F → c1E → c1F', 'c1A → c1E', 'c1F → c1E → c1F']);
  await expect(frame.locator('[data-selection]')).toContainText('Pool of the completed load: c1F, c1E.');
  await expectVersions(frame, ['v1', 'v1', 'v1']);
  await expectValues(frame, 'String', { Retained: '"baseline"', 'Session read': '"baseline"' });
  frame = await showStep(page, 'final-selection', 'cache-hit');
  await expect(frame.locator('[data-outcome]')).toHaveText('Pull completed from cache');
  await expectNoPageOverflow(page);
});

test('all scenario explanations and snapshots survive without JavaScript and in Markdown exports', async ({ browser, baseURL, request }, testInfo) => {
  const context = await browser.newContext({ javaScriptEnabled: false, baseURL, viewport: testInfo.project.use.viewport });
  try {
    const page = await context.newPage();
    await page.goto(route);
    await expect(page.getByLabel('Scenario', { exact: true })).not.toBeVisible();
    await expect(page.getByLabel('Step', { exact: true })).not.toBeVisible();
    const transcript = page.locator('[data-scenario-transcript]');
    await transcript.locator(':scope > summary').click();
    await expect(transcript.locator(':scope > summary')).toHaveText('Read every scenario and value');
    const response = await request.get('internals/workspace-session-state.md');
    expect(response.ok(), 'The Markdown export must be published with the page').toBe(true);
    const markdown = (await response.text()).replace(/\\([.!?])/g, '$1');
    expect(markdown).not.toContain('Previous step');
    expect(markdown).not.toContain('Next step');
    expect(markdown).not.toContain('<select');
    expect(markdown).toMatch(/\*\*Database\*\*\s+v2/);
    for (const item of scenarios) {
      const first = transcript.locator(`[data-frame][data-scenario-id="${item.id}"]`).first();
      const details = first.locator('xpath=ancestor::details[1]');
      await details.locator(':scope > summary').click();
      const title = (await details.locator(':scope > summary').textContent())!.trim();
      expect(markdown).toContain(title);
      for (const id of item.steps) {
        const frame = transcript.locator(`[data-frame][data-scenario-id="${item.id}"][data-step-id="${id}"]`);
        await expect(frame).toBeVisible();
        const heading = (await frame.getByRole('heading', { level: 4 }).textContent())!.trim();
        expect(markdown).toContain(heading);
        expect(markdown).toContain((await frame.locator(':scope > p').first().textContent())!.trim());
        const table = frame.getByRole('table', { name: 'Values after this step', exact: true });
        await expect(table).toBeVisible();
        for (const row of await table.locator('tbody tr').all()) {
          const cells = (await row.locator('th, td').allTextContents()).map((text) => text.trim());
          expect(markdown).toContain(`| ${cells.join(' | ')} |`);
        }
      }
    }
    expect(markdown).toContain('| Role | Database | Connection | Retained | Local edit | Session read | Diff original |');
    expect(markdown.match(/\| Role \| Database \| Connection \| Retained \| Local edit \| Session read \| Diff original \|/g))
      .toHaveLength(scenarios.reduce((count, scenario) => count + scenario.steps.length, 0));
    expect(markdown).toContain('| To-many | {c1B, c1D} | {c1B, c1D} | {c1B} | {c1B, c1C} | {c1B, c1C} | {c1B} |');
    expect(markdown).toContain('ResultError');
    expect(markdown).toContain('| Named result | Query now | Latest Pull | Completed load |');
    expect(markdown).toContain('| Ordered collection | c1F → c1E → c1F | c1A → c1E | c1F → c1E → c1F |');
    expect(markdown).toContain('Security stays with the retained record');
    await expectNoPageOverflow(page);
  } finally {
    await context.close();
  }
});

test('the Markdown export carries the explanation but no interface hints', async ({ request }) => {
  const response = await request.get('internals/workspace-session-state.md');
  expect(response.ok(), 'The Markdown export must be published with the page').toBe(true);
  const markdown = await response.text();
  expect(markdown).toContain('Read every scenario and value');
  expect(markdown, 'Keyboard hints describe the website, not the export').not.toContain('arrow keys');
});
