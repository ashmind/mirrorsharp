import inspector from 'inspector';
import puppeteer, { Page } from 'puppeteer';
import loadModuleByUrl from './render/load-module-by-url';
import loadCSS from './render/load-css';
import type { TestDriver } from '../test-driver';

async function setupRequestInterception(page: Page, html: string) {
    await page.setRequestInterception(true);

    // eslint-disable-next-line @typescript-eslint/no-misused-promises
    page.on('request', async request => {
        const method = request.method();
        const url = new URL(request.url());
        if (method !== 'GET') {
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            await request.abort();
            return;
        }

        if (url.protocol === 'data') {
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

    await setupRequestInterception(page, content);
    // eslint-disable-next-line @typescript-eslint/no-misused-promises
    const loadPromise = new Promise((resolve, reject) => page.exposeFunction(
        'notifyLoaded', (e?: Error) => e ? reject(e) : resolve())
    );
    if (debug)
        debugger; // eslint-disable-line no-debugger

    // does not exist -- required for module relative references
    await page.goto('http://mirrorsharp.test');

    if (debug)
        debugger; // eslint-disable-line no-debugger

    await loadPromise;
    const screenshot = await page.screenshot();

    console.log('before close');
    await browser.close();
    console.log('after close');

    return screenshot;
}