import execa from 'execa';
import fetch from 'node-fetch';
import { setTimeout  } from '../../real-timers';
import { shouldSkipRender } from '../should-skip';

let container: undefined | {
    id: string;
    port: string;
};

async function waitFor(ready: () => Promise<boolean>|boolean, error: () => Error) {
    // console.log('waitFor: starting');
    let remainingRetryCount = 50;
    // console.log(`[${remainingRetryCount} retries] waitFor: await Promise.resolve(ready())`);
    while (!(await Promise.resolve(ready()))) {
        if (remainingRetryCount === 0) {
            // console.log(`[${remainingRetryCount} retries] waitFor: error - no tries remaining`);
            throw error();
        }
        // console.log(`[${remainingRetryCount} retries] waitFor: await new Promise(() => setTimeout())`);
        await new Promise(resolve => setTimeout(resolve, 100));
        remainingRetryCount -= 1;
        // console.log(`[${remainingRetryCount} retries] waitFor: await Promise.resolve(ready())`);
    }
    // console.log('waitFor: completed');
}

export default async (): Promise<{ port: string }> => {
    if (shouldSkipRender)
        throw new Error('Setup should not be called if we are skipping render.');

    if (container)
        return { port: container.port };

    const containerId = (await execa('docker', [
        'container',
        'run',
        '-d',
        '-p',
        '9222',
        '--rm',
        '--security-opt',
        `seccomp=${__dirname}/chrome.seccomp.json`,
        'gcr.io/zenika-hub/alpine-chrome:108',
        '--remote-debugging-address=0.0.0.0',
        `--remote-debugging-port=9222`,
        'about:blank'
    ])).stdout;
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    const port = (await execa('docker', [
        'port',
        containerId,
        '9222'
    ])).stdout.match(/:(\d+)$/)![1];
    console.log(`Started Chrome container ${containerId} on port ${port}. Open http://localhost:${port} to debug.`);

    await waitFor(async () => {
        try {
            // console.log('lazySetup: waitFor, await fetch()');
            await fetch(`http://localhost:${port}`);
            // console.log('lazySetup: waitFor, fetch completed');
            return true;
        }
        catch {
            // console.log('lazySetup: waitFor, fetch failed');
            return false;
        }
    }, () => new Error(`Chrome container has not opened port ${port} within the wait period.`));

    container = { id: containerId, port };
    return { port };
};

afterAll(async () => {
    if (!container)
        return;

    await execa('docker', ['stop', container.id]);
    console.log('Stopped Chrome container', container.id);
});