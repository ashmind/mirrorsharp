import inspector from 'inspector';
import puppeteer, { HTTPRequest, Page } from 'puppeteer';
import loadCSS from './render/load-css';
import loadJSOrTS from './render/load-js-or-ts';
import type { TestDriver } from '../test-driver';
import lazyRenderSetup from './render/docker/lazy-setup';
import * as console from 'console';
import { setTimeout } from './real-timers';

/* eslint-disable @typescript-eslint/no-unused-vars */
const verboseLogger = () => {
    const start = Date.now();
    return (...args: ReadonlyArray<unknown>) => {
        const seconds = Math.round((Date.now() - start) / 1000);
        // console.log(`[${seconds}s]`, ...args);
    };
};
/* eslint-restore @typescript-eslint/no-unused-vars */

async function processRequest(request: HTTPRequest, html: string) {
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

async function setupRequestInterception(
    page: Page, html: string,
    onRequestError: (e: unknown) => void
) {
    await page.setRequestInterception(true);

    page.on('request', request => {
        processRequest(request, html)
            .catch(onRequestError);
    });
}

const timeout = (ms: number, message: string) => new Promise(
    (_, reject) => setTimeout(() => { reject(new Error(message)); }, ms)
);

export { shouldSkipRender } from './render/should-skip';

//let renderInProgress = false;
export default async function render(
    driver: TestDriver<unknown>,
    size: { width: number; height: number },
    { debug = !!inspector.url()/*, seconds = () => 0*/ }: { debug?: boolean/*; seconds?: () => number*/ } = {}
) {
    const verbose = verboseLogger();

    // if (renderInProgress)
    //     throw 'Attempted to start a new render while render is already in progress';
    // renderInProgress = true;
    // try {
    verbose(`render: starting`);
    verbose(`render: await lazyRenderSetup()`);
    const { port } = await lazyRenderSetup();

    const content = `<!DOCTYPE html>
        <html>
        <head>
            <title>mirrorsharp render test page</title>
            <link rel="stylesheet" href="css/mirrorsharp.css">
        </head>
        <body>
            <script>
                window.addEventListener('error', e => {
                    console.error(e);
                    notifyLoaded(\`$\{e.message}\n  at $\{e.filename}\`);
                });
            </script>
            <script type="module">
                import { timers } from './ts/testing/helpers/render/browser-fake-timers.ts';
                import { TestDriver, setTimers } from './ts/testing/test-driver-isomorphic.ts';

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

    verbose(`render: await puppeteer.connect()`);
    const browser = await puppeteer.connect({ browserURL: `http://localhost:${port}` });
    verbose(`render: await browser.newPage()`);
    const page = await browser.newPage();
    verbose(`render: await page.setViewport()`);
    await page.setViewport(size);

    let resolveLoad!: () => void;
    let rejectLoad!: (reason: unknown) => void;
    const loadPromise = new Promise<void>((resolve, reject) => [resolveLoad, rejectLoad] = [resolve, reject]);

    verbose(`render: await setupRequestInterception()`);
    await setupRequestInterception(page, content, e => rejectLoad(e));
    verbose(`render: await page.exposeFunction()`);
    await page.exposeFunction('notifyLoaded', (e?: string) => e ? rejectLoad(e) : resolveLoad());

    // does not exist -- required for module relative references
    verbose(`render: await page.goto()`);
    await page.goto('http://mirrorsharp.test');

    verbose(`render: await loadPromise`, 'debug', debug);
    await Promise.race(!debug ? [
        loadPromise,
        timeout(30000, 'Page did not call notifyLoaded() within the time limit.')
    ] : [loadPromise]);

    verbose(`render: await page.screenshot()`);
    const screenshot = await page.screenshot();

    verbose(`render: wait page.close()`);
    await page.close();
    verbose(`render: await browser.disconnect()`);
    browser.disconnect();

    verbose(`render: completed`);
    return screenshot;
    // }
    // finally {
    //     renderInProgress = false;
    // }
}