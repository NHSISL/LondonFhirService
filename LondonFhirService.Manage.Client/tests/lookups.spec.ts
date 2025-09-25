import { test, expect } from '@playwright/test';

const lookupName = "Test Lookup Name";
const editedLookupName = "Test Lookup Name EDITED";
const lookupValue = "Test Lookup Value";
const editedLookupValue = "Test Lookup Value EDITED";
const lookupGroupValue = "Test Lookup Value Value";
const editedLookupGroupValue = "Test Lookup Value Value EDITED";

test.beforeEach(async ({ page }) => {
    await page.goto('https://localhost:6073/configuration/lookups');
});

test('has Lookup Configuration heading', async ({ page }) => {
    const heading = page.getByRole('heading', { name: 'Lookup Configuration' });
    await expect(heading).toBeVisible();
});

test('can add, edit, and delete a lookup', async ({ page }) => {
    await addLookup(page, lookupName, lookupValue, lookupGroupValue);
    await editLookup(page, lookupName, editedLookupName, editedLookupValue, editedLookupGroupValue);
    await deleteLookup(page, editedLookupName);
});

async function addLookup(page, name, value, groupValue) {
    const addLookupButton = page.getByRole('button', { name: 'New' });
    await addLookupButton.click();

    const nameInput = page.getByPlaceholder('Lookup Name');
    await nameInput.fill(name);

    const valueInput = page.getByPlaceholder('Lookup Value');
    await valueInput.fill(value);

    const groupValueInput = page.getByPlaceholder('Lookup Group Value');
    await groupValueInput.fill(groupValue);

    const addLookupRecordButton = page.getByRole('button', { name: 'Add' });
    await addLookupRecordButton.click();

    //const lookupTableCell = page.getByRole('cell', { name });
    //await expect(lookupTableCell).toBeVisible();
}

async function editLookup(page, oldName, newName, newValue, newGroupValue) {
    const searchLookups = page.getByPlaceholder('Search lookups');
    await searchLookups.fill(oldName);
    await page.waitForTimeout(3000);

    const editLookupRecordButton = page.getByRole('button', { name: 'Edit' });
    await editLookupRecordButton.click();

    const nameInput = page.getByPlaceholder('Lookup Name');
    await nameInput.fill(newName);

    const valueInput = page.getByPlaceholder('Lookup Value');
    await valueInput.fill(newValue);

    const groupValueInput = page.getByPlaceholder('Lookup Group Value');
    await groupValueInput.fill(newGroupValue);

    const updateLookupRecordButton = page.getByRole('button', { name: 'Update' });
    await updateLookupRecordButton.click();

    const editedLookupTableCell = page.getByRole('cell', { name: newName });
    await expect(editedLookupTableCell).toBeVisible();
}

async function deleteLookup(page, name) {
    const searchLookups = page.getByPlaceholder('Search lookups');
    await searchLookups.fill(name);
    await page.waitForTimeout(1000);

    const deleteLookupRecordButton = page.getByRole('button', { name: 'Delete' });
    await deleteLookupRecordButton.click();

    const confirmDeleteLookupRecordButton = page.getByRole('button', { name: 'Yes, Delete' });
    await confirmDeleteLookupRecordButton.click();

    const clearSearchLookups = page.getByPlaceholder('Search lookups');
    await clearSearchLookups.fill("");
    await page.waitForTimeout(1000);

    const deletedLookupTableCell = page.getByRole('cell', { name });
    await expect(deletedLookupTableCell).not.toBeVisible();
}