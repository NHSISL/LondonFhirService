import { test, expect } from '@playwright/test';

test.beforeEach(async ({ page }) => {
    await page.goto('https://localhost:6073/home');
});

test('has title', async ({ page }) => {
    await expect(page).toHaveTitle("Re-Identification Management");
});

test('has navigation', async ({ page }) => {
    await page.waitForTimeout(10000);
    await expect(page.getByRole('link', { name: 'User Access' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Access Audit' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Csv Identification' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'PDS Data' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Ods Data' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Config - Lookup Reasons' })).toBeVisible();
});