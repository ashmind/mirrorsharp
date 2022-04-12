import execa from 'execa';
import fetch from 'node-fetch';
import { setTimeout  } from '../../real-timers';
import { setContainerIdFromSetup } from './container-id';
import { shouldSkipRender } from '../should-skip';

type RenderSetupState = 'none' | 'pending' | 'ready';
function setSetupState(state: RenderSetupState) {
    process.env.TEST_DOCKER_SETUP_STATE = state;
    // console.log('TEST_DOCKER_SETUP_STATE set to', state);
}
function getSetupState() {
    return (process.env.TEST_DOCKER_SETUP_STATE as RenderSetupState|undefined) ?? 'none';
}
function setPort(port: string) {
    process.env.TEST_DOCKER_PORT = port;
}
function getPort() {
    return process.env.TEST_DOCKER_PORT;
}

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

    if (getSetupState() === 'ready')
        return { port: getPort()! };

    if (getSetupState() === 'pending') {
        await waitFor(
            () => getSetupState() === 'ready',
            () => new Error(`Pending setup has not completed within the wait period.`)
        );
        return { port: getPort()! };
    }

    setSetupState('pending');
    const chromeContainerId = (await execa('docker', [
        'container',
        'run',
        '-d',
        '-p',
        '9222',
        '--rm',
        '--security-opt',
        `seccomp=${__dirname}/chrome.seccomp.json`,
        'gcr.io/zenika-hub/alpine-chrome:86',
        '--remote-debugging-address=0.0.0.0',
        `--remote-debugging-port=9222`,
        'about:blank'
    ])).stdout;
    setContainerIdFromSetup(chromeContainerId);
    const port = (await execa('docker', [
        'port',
        chromeContainerId,
        '9222'
    ])).stdout.match(/:(\d+)$/)![1];
    console.log(`Started Chrome container ${chromeContainerId} on port ${port}. Open http://localhost:${port} to debug.`);
    setPort(port);

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

    setSetupState('ready');
    return { port };
};