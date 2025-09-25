import { BrowserContext } from '@playwright/test';

export async function createNetworkInterceptor(context) {
    await context.route('**/login.microsoftonline.com/**', (route) => {
        if (route.request().method() === 'POST') {
            route.fulfill({
                contentType: 'application/json',
                body: JSON.stringify({
                    token_type: 'Bearer',
                    scope: 'openid profile email User.Read',
                    expires_in: 50000,
                    ext_expires_in: 50000,
                    access_token: '',
                    refresh_token: '',
                    id_token: '',
                    client_info: ''
                })
            });
        } else {
            route.continue();
        }
    });
}