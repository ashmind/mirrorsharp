import { transformFileAsync } from '@babel/core';
import fg from 'fast-glob';
import jetpack from 'fs-jetpack';
import { task, exec, build } from 'oldowan';
// @ts-expect-error (https://github.com/microsoft/TypeScript/issues/38149)
import { addImportExtensions } from './build/plugins/add-import-extensions.ts';
import './build/storybook.ts';

const clean = task('clean', async () => {
    const paths = await fg(['.temp/**/*.*', 'dist/**/*.*', '!dist/node_modules/**/*.*']);
    await Promise.all(paths.map(jetpack.removeAsync));
});

const depsDepcheck = task('deps:depcheck', () => exec('depcheck'));
const deps = task('deps', () => Promise.all([
    depsDepcheck()
]));

const tsESLint = task('ts:eslint', async () => {
    // https://github.com/import-js/eslint-import-resolver-typescript/issues/208
    if (import.meta.url.includes('%23')) {
        console.warn("Linting is not possible ('#' in path).");
        return;
    }

    await exec('eslint ./src --max-warnings 0 --ext .js,.jsx,.ts,.tsx');
});
const tsUnusedExports = task('ts:unused-exports', async () => {
    console.log('ts-unused-exports: dist');
    await exec('ts-unused-exports ./src/tsconfig.json --ignoreFiles=\\.(stories|tests)$ --ignoreFiles=test\\.data --ignoreFiles=testing');
    console.log('ts-unused-exports: tests');
    await exec('ts-unused-exports ./src/tsconfig.json --excludePathsFromReport=stories');
});

const tscArgs = '--project ./src/tsconfig.build.json --outDir ./.temp';
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
                addImportExtensions
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

const css = task('css',
    () => jetpack.copyAsync('src', 'dist', { matching: ['*.css'], overwrite: true }),
    { watch: ['src/*.css'] }
);

const files = task('files', async () => {
    await jetpack.copyAsync('./README.md', 'dist/README.md', { overwrite: true });

    // It's almost impossible to reliably convince npm not to install
    // devDependencies (e.g. in SharpLab), so it is easier to remove them.
    const packageJson = await jetpack.readAsync('./package.json', 'json') as {
        devDependencies?: Record<string, string>;
    };
    delete packageJson.devDependencies;
    await jetpack.writeAsync('dist/package.json', packageJson);
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