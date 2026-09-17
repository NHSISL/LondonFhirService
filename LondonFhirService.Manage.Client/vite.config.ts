import { fileURLToPath, URL } from 'node:url';

// From 'vitest/config' rather than 'vite', which is the same defineConfig plus the `test` key
// below. Importing it from 'vite' compiles but drops that key on the floor.
import { defineConfig } from 'vitest/config';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';

const baseFolder =
    env.APPDATA !== undefined && env.APPDATA !== ''
        ? `${env.APPDATA}/ASP.NET/https`
        : `${env.HOME}/.aspnet/https`;

const certificateName = "LondonFhirService.Manage.Client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(baseFolder)) {
    fs.mkdirSync(baseFolder, { recursive: true });
}

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (0 !== child_process.spawnSync('dotnet', [
        'dev-certs',
        'https',
        '--export-path',
        certFilePath,
        '--format',
        'Pem',
        '--no-password',
    ], { stdio: 'inherit', }).status) {
        throw new Error("Could not create certificate.");
    }
}

// Launching through Visual Studio sets one of the two environment variables, so the fallback only
// applies to `npm run dev` on its own. It has to be this client's own host - LondonFhirService.Manage
// on 6284, per its launchSettings - not LondonFhirService.Api on 7284, which serves a different API
// and has no /api/providers, /api/audits or /api/metrics to proxy to.
const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:6284';

// https://vitejs.dev/config/
export default defineConfig({
    // Unit tests are the *.test.ts files under src. Spelling that out matters: vitest's default
    // include also matches tests/*.spec.ts, which are Playwright specs against a running browser -
    // they fail at collection, and an unfiltered `vitest` run reported three failed files for that
    // reason alone. The narrow include is also what lets the npm script drop its positional
    // filename filter, which was quietly reducing the suite to a third of its files.
    //
    // environment lives here rather than as a --dom flag on the script so a bare `npx vitest` and
    // `npm test` agree about what the DOM is.
    test: {
        environment: 'happy-dom',
        include: ['src/**/*.test.{ts,tsx}']
    },
    plugins: [plugin()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        proxy: {
            '^/odata/*': {
                target,
                secure: false
            },
            '^/api/*': {
                target,
                secure: false
            }
        },
        port: 6073,
        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        }
    }
})
