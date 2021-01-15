import inspector from 'inspector';
import puppeteer, { Page } from 'puppeteer';
import loadModuleByUrl from './render/load-module-by-url';
import loadCSS from './render/load-css';
import type { TestDriver } from '../test-driver';

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
            body: await loadCSS(url)
        });
        return;
    }

    const content = await loadModuleByUrl(url);
    await request.respond({
        headers: {
            'Content-Type': 'application/javascript'
        },
        body: content
    });
}

async function setupRequestInterception(page: Page, html: string, onRequestError: (e: unknown) => void) {
    await page.setRequestInterception(true);

    page.on('request', request => {
        processRequest(request, html)
            .catch(onRequestError);
    });
}

export default async function render(
    driver: TestDriver<unknown>,
    size: { width: number; height: number },
    { debug = !!inspector.url() }: { debug?: boolean } = {}
) {
    const content = `<!DOCTYPE html>
        <html>
          <head>
            <title>mirrorsharp render test page</title>
            <link rel="stylesheet" href="./css/mirrorsharp.css">
          </head>
          <body>
            <script type="module">
                import { TestDriver } from './tests/test-driver-isomorphic';

                TestDriver
                    .fromJSON(${JSON.stringify(driver.toJSON())})
                    .then(() => notifyLoaded(), e => {
                        console.error(e);
                        notifyLoaded(e.message);
                    });
            </script>
          </body>
        </html>`;

    const browser = await puppeteer.launch({
        headless: !debug,
        devtools: debug
    });
    const [page] = await browser.pages();
    await page.setViewport(size);

    // eslint-disable-next-line @typescript-eslint/no-misused-promises
    const requestErrors = [] as Array<unknown>;
    await setupRequestInterception(page, content, e => {
        console.error('Error during request', e);
        requestErrors.push(e);
    });

    let load: { resolve: () => void; reject: ((e: unknown) => void) } | undefined;
    const loadPromise = new Promise((resolve, reject) => { load = { resolve, reject }; });
    await page.exposeFunction('notifyLoaded', (e?: Error) => e ? load!.reject(e) : load!.resolve());
    if (debug)
        debugger; // eslint-disable-line no-debugger

    // does not exist -- required for module relative references
    await page.goto('http://mirrorsharp.test');

    if (debug)
        debugger; // eslint-disable-line no-debugger

    await loadPromise;
    if (requestErrors.length > 0)
        throw requestErrors[0];
    const screenshot = await page.screenshot();

    await browser.close();

    return screenshot;
}