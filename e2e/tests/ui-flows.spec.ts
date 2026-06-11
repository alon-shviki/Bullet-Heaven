import { test, expect } from '@playwright/test';

test.describe('Main Menu', () => {
  test('game page loads and shows main menu', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('.menu-logo')).toBeVisible();
    await expect(page.locator('.menu-logo')).toContainText('BULLET HEAVEN');
  });

  test('canvas element renders', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('canvas')).toBeVisible();
  });

  test('Start Game button is present', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('button.btn-normal', { hasText: 'Start Game' })).toBeVisible();
  });

  test('Leaderboard button is present', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('button.btn-leaderboard')).toBeVisible();
  });
});

test.describe('Navigation', () => {
  test('clicking Start Game hides the main menu overlay', async ({ page }) => {
    await page.goto('/');
    await page.locator('button.btn-normal', { hasText: 'Start Game' }).click();
    await expect(page.locator('.menu-overlay')).not.toBeVisible();
  });

  test('clicking Leaderboard shows leaderboard overlay', async ({ page }) => {
    await page.goto('/');
    await page.locator('button.btn-leaderboard').click();
    // the leaderboard reuses the full-screen codex overlay, not the menu overlay
    await expect(page.locator('.codex-overlay')).toBeVisible();
    await expect(page.locator('.codex-title')).toHaveText('LEADERBOARD');
  });
});
