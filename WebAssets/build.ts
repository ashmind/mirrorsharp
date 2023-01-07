import jetpack from 'fs-jetpack';
import fg from 'fast-glob';
import { transformFileAsync } from '@babel/core';
import { task, exec, build } from 'oldowan';
import './build/storybook';

const clean = task('clean', async () => {
    const paths = await fg(['.temp/**/*.*', 'dist/**/*.*', '!dist/node_modules/**/*.*']);
    await Promise.all(paths.map(jetpack.removeAsync));
});

const depsDepcheck = task('deps:depcheck', () => exec('depcheck'));
const deps = task('deps', () => Promise.all([
    depsDepcheck()
]));

const tsESLint = task('ts:eslint', () => exec('eslint ./ts --max-warnings 0 --ext .js,.jsx,.ts,.tsx'));
const tsUnusedExports = task('ts:unused-exports', async () => {
    console.log('ts-unused-exports: dist');
    await exec('ts-unused-exports ./ts/tsconfig.json --ignoreFiles=\\.(stories|tests)$ --ignoreFiles=test\\.data --ignoreFiles=testing');
    console.log('ts-unused-exports: tests');
    await exec('ts-unused-exports ./ts/tsconfig.json --excludePathsFromReport=stories');
});

const tscArgs = '--project ./ts/tsconfig.build.json --outDir ./.temp';
const tsTsc = task('ts:tsc',
    () => exec(`tsc ${tscArgs}`),
    { watch: () => exec(`tsc --watch ${tscArgs}`) }
);

const tsCopyDeclarations = task('ts:copy-declarations',
    () => jetpack.copyAsync('./.temp', './dist', { matching: '*.d.ts', overwrite: true })
);

const tsTransform = task('ts:transform', async () => {
    await Promise.all((await fg(['.temp/**/*.js'])).map(async path => {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        const { code: transformed } = (await transformFileAsync(path, {
            plugins: [
                // Add .js extension to all imports.
                // Technically TypeScript already resolves .js to .ts, but it's a hack.
                'babel-plugin-add-import-extension'
            ]
        }))!;

        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        await jetpack.writeAsync(path.replace('.temp', 'dist'), transformed!);
    }));
}, { watch: ['.temp/**/*.js'] });

const ts = task('ts', async () => {
    await Promise.all([
        await tsESLint(),
        await tsUnusedExports()
    ]);
    await tsTsc();
    await Promise.all([
        tsTransform(),
        tsCopyDeclarations()
    ]);
});

const css = task('css', () => jetpack.copyAsync('css', 'dist', { overwrite: true }), { watch: ['css/*.*'] });

const files = task('files', async () => {
    await jetpack.copyAsync('./README.md', 'dist/README.md', { overwrite: true });
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    const packageJson = JSON.parse((await jetpack.readAsync('./package.json'))!) as {
        type?: 'module';
        devDependencies?: Record<string, string>;
    };
    // cannot be specified in current package.json due to https://github.com/TypeStrong/ts-node/issues/935
    // which is fine, from perspective of the project itself it's TypeScript, so type=module is irrelevant
    // only the output (dist) is JS modules
    packageJson.type = 'module';
    delete packageJson.devDependencies;
    await jetpack.writeAsync('dist/package.json', JSON.stringify(packageJson, null, 4));
}, { watch: ['./README.md', './package.json'] });

task ('lint', async () => {
    await tsESLint();
    await tsUnusedExports();
    await tsTsc();
    await depsDepcheck();
});

task('default', async () => {
    await clean();
    await Promise.all([
        deps(),
        ts(),
        css(),
        files()
    ]);
});

// eslint-disable-next-line @typescript-eslint/no-floating-promises
build();