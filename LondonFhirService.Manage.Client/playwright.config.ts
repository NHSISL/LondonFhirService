import { defineConfig, devices } from '@playwright/test';
import path from 'path';
import { fileURLToPath } from 'url';
import dotenv from 'dotenv';
import { exit } from 'process';

const __filename = fileURLToPath(import.meta.url); // get the resolved path to the file
const __dirname = path.dirname(__filename); // get the name of the directory

const authFile = path.join(__dirname, './playwright/.auth/user.json');

dotenv.config({ path: path.resolve(__dirname, '.env') });
dotenv.config({ path: path.resolve(__dirname, '.env.local') });

if (!process.env.TEST_USERNAME || !process.env.TEST_PASSWORD) {
    console.log("\x1b[31m");
    console.log("Please set TEST_USERNAME and TEST_PASSWORD environment variables.");
    console.log("\x1b[0m");
    console.log("Either set in .env.local file.");
    console.log("or pass as argument:");
    console.log("BASH: \n 'TEST_USERNAME=me TEST_PASSWORD=secret npx playwright test'");
    console.log("POWERSHELL:");
    console.log(" $env:TEST_USERNAME=\"me\"");
    console.log(" $env:TEST_PASSWORD=\"secret\"");
    console.log("npx playwright test");
    console.log("NOTE: passwords must be base64 encoded (btoa()) (to avoid problems with escape character)");
    console.log("");
    exit(1);
}

try {
    atob(process.env.TEST_PASSWORD);
} catch (ex) {
    console.log("\x1b[31m");
    console.log("TEST_PASSWORD is not base64 encoded. Please encode the password with btoa() before setting the environment variable.");
    console.log("\x1b[0m");
    console.log(ex);
    exit(1);
}

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
    testDir: './tests',
    /* Run tests in files in parallel */
    fullyParallel: true,
    /* Fail the build on CI if you accidentally left test.only in the source code. */
    forbidOnly: !!process.env.CI,
    /* Retry on CI only */
    retries: process.env.CI ? 2 : 0,
    /* Opt out of parallel tests on CI. */
    workers: process.env.CI ? 1 : undefined,
    /* Reporter to use. See https://playwright.dev/docs/test-reporters */
    reporter: 'html',
    /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
    use: {
        /* Base URL to use in actions like `await page.goto('/')`. */
        // baseURL: 'http://127.0.0.1:3000',

        /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
        trace: 'on-first-retry',
    },

    /* Configure projects for major browsers */
    projects: [
        { name: 'setup', testMatch: /.*\.setup\.ts/ },
        {
            name: 'Microsoft Edge',
            use: { ...devices['Desktop Edge'], channel: 'msedge', storageState: authFile, },
            dependencies: ['setup']
        }
        //{
        //    name: 'Google Chrome',
        //    use: { ...devices['Desktop Chrome'], channel: 'chrome' },
        //},
    ],

    /* Run your local dev server before starting the tests */
    webServer: {
        command: "dotnet run --project ../LondonDataServices.IDecide.Manage/",
        url: 'https://localhost:6073/',
        reuseExistingServer: true,
        ignoreHTTPSErrors: true,
    },
});