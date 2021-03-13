import inspector from 'inspector';
import puppeteer, { Page } from 'puppeteer';
import loadCSS from './render/load-css';
import loadJSOrTS from './render/load-js-or-ts';
import type { TestDriver } from '../test-driver';
import lazyRenderSetup from './render/docker/lazy-setup';
import controlledPromise from '../../ts/helpers/controlled-promise';

async function processRequest(request: puppeteer.Request, html: string) {
    const method = request.method();
    const url = new URL(request.url());
    if (method !== 'GET') {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        await request.abort();
        return;
    }

    if (url.protocol === 'data:') {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        await request.continue();
        return;
    }

    if (url.pathname === '/') {
        await request.respond({
            headers: {
                'Content-Type': 'text/html'
            },
            body: html
        });
        return;
    }

    if (url.pathname === '/favicon.ico') {
        await request.respond({ status: 404 });
        return;
    }

    if (url.pathname.endsWith('.css')) {
        await request.respond({
            headers: {
                'Content-Type': 'text/css'
            },
            body: await loadCSS(url.pathname)
        });
        return;
    }

    if (/\.(js|cjs|mjs|ts)$/.test(url.pathname)) {
        await request.respond({
            headers: {
                'Content-Type': 'application/javascript'
            },
            body: await loadJSOrTS(url.pathname)
        });
        return;
    }

    console.warn(`Could not find ${url.toString()} - returning 404.`);
    await request.respond({ status: 404 });
}

async function setupRequestInterception(page: Page, html: string, onRequestError: (e: unknown) => void) {
    await page.setRequestInterception(true);

    page.on('request', request => {
        processRequest(request, html)
            .catch(onRequestError);
    });
}

const timeout = (ms: number, message: string) => new Promise(
    (_, reject) => setTimeout(() => reject(new Error(message)), ms)
);

export { shouldSkipRender } from './render/should-skip';
export default async function render(
    driver: TestDriver<unknown>,
    size: { width: number; height: number },
    { debug = !!inspector.url() }: { debug?: boolean } = {}
) {
    const { port } = await lazyRenderSetup();

    const content = `<!DOCTYPE html>
        <html>
          <head>
            <title>mirrorsharp render test page</title>
            <link rel="stylesheet" href="css/mirrorsharp.css">
          </head>
          <body>
            <script type="module">
                import { timers } from './tests/helpers/render/browser-fake-timers.ts';
                import { TestDriver, setTimers } from './tests/test-driver-isomorphic.ts';

                setTimers(timers);
                TestDriver
                    .fromJSON(${JSON.stringify(driver.toJSON())})
                    .then(() => notifyLoaded(), e => {
                        console.error(e);
                        notifyLoaded(e.message);
                    });
            </script>
          </body>
        </html>`;

    const browser = await puppeteer.connect({ browserURL: `http://localhost:${port}` });
    const page = await browser.newPage();
    await page.setViewport(size);

    const load = controlledPromise();
    await setupRequestInterception(page, content, e => load.reject(e));
    await page.exposeFunction('notifyLoaded', (e?: Error) => e ? load.reject(e) : load.resolve());

    // does not exist -- required for module relative references
    await page.goto('http://mirrorsharp.test');

    await Promise.race(!debug ? [
        load.promise,
        timeout(30000, 'Page did not call notifyLoaded() within the time limit.')
    ] : [load.promise]);
    const screenshot = await page.screenshot();

    await page.close();
    browser.disconnect();

    return screenshot;
}