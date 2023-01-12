import { createRequire } from 'module';
import { fileURLToPath } from 'node:url';
import path from 'path';
import { promisify } from 'util';
import execa from 'execa';
import jetpack from 'fs-jetpack';
import { task } from 'oldowan';
import kill from 'tree-kill';
import waitOn from 'wait-on';

const require = createRequire(import.meta.url);
const resolvePlaywrightPath = (relativePath: string) => path.resolve(
    path.dirname(require.resolve('playwright-core')), relativePath
);
const getPlaywrightVersion = async () => ((await jetpack.readAsync(
    resolvePlaywrightPath('package.json'), 'json'
)) as { version: string }).version;

export const root = path.resolve(fileURLToPath(new URL('.', import.meta.url)), '..');
export const sourceRoot = path.resolve(root, 'src');

const UPDATE_SNAPSHOTS_KEY = 'SHARPLAB_TEST_UPDATE_SNAPSHOTS';

const exec2 = (command: string, args?: ReadonlyArray<string>) => execa(command, args, {
    preferLocal: true,
    stdout: process.stdout,
    stderr: process.stderr
});

task('storybook:test:in-container', async () => {
    console.log('http-server: starting');
    const server = exec2('http-server', ['storybook-static', '--port', '6006', '--silent']);
    try {
        await waitOn({
            resources: ['http://localhost:6006'],
            timeout: 120000
        });
        console.log('http-server: ready');

        const updateSnapshots = process.env[UPDATE_SNAPSHOTS_KEY] === 'true';
        console.log(`Starting Storybook tests${updateSnapshots ? ' (with snapshot update)' : ''}...`);
        await exec2('test-storybook', [
            '--stories-json',
            ...(updateSnapshots ? ['--updateSnapshot'] : [])
        ]);
    }
    finally {
        if (!server.killed) {
            console.log('http-server: terminating');
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            await promisify(kill)(server.pid!);
        }
    }
}, {
    timeout: 20 * 60 * 1000
});

const test = task('storybook:test', async () => {
    const playwrightVersion = await getPlaywrightVersion();
    const containerName = `playwright:v${playwrightVersion}-focal`;
    console.log(`Starting ${containerName} in Docker...`);
    await exec2('docker', [
        'run',
        '--rm',
        '--ipc=host',
        `--volume=${root}:/work`,
        '--workdir=/work',
        ...(process.env[UPDATE_SNAPSHOTS_KEY] === 'true' ? ['--env', `${UPDATE_SNAPSHOTS_KEY}=true`] : []),
        // Note: Playwright version must match dependency of @storybook/test-runner
        `mcr.microsoft.com/${containerName}`,
        './node_modules/.bin/ts-node-esm', './build.ts', 'storybook:test:in-container'
    ]);
}, {
    timeout: 20 * 60 * 1000
});

const clean = task('storybook:test:clean', async () => {
    const snapshotPaths = await jetpack.findAsync(sourceRoot, {
        matching: '**/__image_snapshots__/**'
    });
    for (const path of snapshotPaths) {
        await jetpack.removeAsync(path);
    }
});

task('storybook:test:update', async () => {
    await clean();
    process.env[UPDATE_SNAPSHOTS_KEY] = 'true';
    await test();
});