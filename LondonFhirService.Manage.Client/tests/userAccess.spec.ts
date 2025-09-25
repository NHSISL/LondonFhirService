import { test, expect } from '@playwright/test';

test.beforeEach(async ({ page }) => {
    await page.goto('https://localhost:6073/userAccess');
});

test('has User Access heading', async ({ page }) => {
    const heading = page.getByRole('heading', { name: 'User Access' });
    await expect(heading).toBeVisible();
});

test('has CanAddUser', async ({ page }) => {
    const getAddUserButton = page.getByRole('button', { name: 'Add New User' });
    await getAddUserButton.click();

    const emailAddressInput = page.getByPlaceholder('Enter email address');
    await emailAddressInput.fill("david.hayes17@nhs.net");
    const getUserButton = page.getByRole('cell', { name: 'david.hayes17@nhs.net' });
    await getUserButton.click();

    const AddOrganisationClick = page.getByRole('checkbox').first();
    await AddOrganisationClick.click();

    const AddUserButton = page.getByRole('button', { name: 'Save' });
    await AddUserButton.click();

    const EditUserButton = page.getByRole('row', { name: 'HAYES, David (NHS North East'}).getByRole('button');
    await EditUserButton.click();

    const RemoveOrganisationClick = page.getByRole('checkbox').first();
    await RemoveOrganisationClick.click();

    const RemoveUserButton = page.getByRole('button', { name: 'Remove User' });
    await RemoveUserButton.click();

});