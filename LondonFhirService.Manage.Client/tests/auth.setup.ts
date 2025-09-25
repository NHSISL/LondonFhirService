import { test as setup, expect } from '@playwright/test';
import path from 'path';
import { fileURLToPath } from 'url';
import dotenv from 'dotenv';

const __filename = fileURLToPath(import.meta.url); // get the resolved path to the file
const __dirname = path.dirname(__filename); // get the name of the directory
dotenv.config({ path: path.resolve(__dirname, '.env') });;
dotenv.config({ path: path.resolve(__dirname, '.env.local') });;
const authFile = path.join(__dirname, '../playwright/.auth/user.json');


setup('authenticate', async ({ page }) => {
    // Perform authentication steps. Replace these actions with your own.
    await page.goto('https://localhost:6073/');
    await page.getByRole('button', { name: 'Sign in' }).click();
    await page.getByLabel('yourusername@nelcsu.nhs.uk').fill(process.env.TEST_USERNAME);
    await page.getByRole('button', { name: 'Next' }).click();
    await page.getByLabel('Password').fill(atob(process.env.TEST_PASSWORD));
    await page.getByRole('button', { name: 'Sign in' }).click();
    await page.getByRole('button', { name: 'Yes' }).click();
    await page.waitForURL('https://localhost:6073/');
    // Alternatively, you can wait until the page reaches a state where all cookies are set.
    await expect(page.getByRole('heading', { name: 'Welcome to Re-Identification' })).toBeVisible();

    // End of authentication steps.

    await page.context().storageState({ path: authFile });
});