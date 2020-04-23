import jetpack from 'fs-jetpack';
import execa from 'execa';
import fg from 'fast-glob';
import babel from '@babel/core';
import oldowan from 'oldowan';
import convertPrivateFields from './build/babel-plugin-convert-private-fields-to-symbols.js';
const { task, tasks, run } = oldowan;

task('clean', async () => {
    const paths = await fg(['dist/**/*.*', '!dist/node_modules/**/*.*']);
    await Promise.all(paths.map(jetpack.removeAsync));
});

task('ts', async () => {
    await execa.command('eslint ./ts --max-warnings 0 --ext .js,.jsx,.ts,.tsx', {
        preferLocal: true,
        stdout: process.stdout,
        stderr: process.stderr
    });

    await execa.command('tsc --project ./ts/tsconfig.json --module ES2015 --noEmit false --outDir ./dist --declaration true', {
        preferLocal: true,
        stdout: process.stdout,
        stderr: process.stderr
    });

    await Promise.all((await fg(['dist/**/*.js'])).map(async path => {
        const { code: transformed } = /** @type {import('@babel/core').BabelFileResult} */(await babel.transformFileAsync(path, {
            plugins: [
                // Add .js extension to all imports.
                // Technically TypeScript already resolves .js to .ts, but it's a hack.
                'babel-plugin-add-import-extension',

                '@babel/plugin-syntax-class-properties',
                // Sort out private class fields which are level 3 proposal and
                // should not be posted to npm. Technically TypeScript can do it,
                // but I think WeakMap is an absolute overkill.
                convertPrivateFields
            ]
        }));

        await jetpack.writeAsync(path, /** @type {string} */(transformed));
    }));
}, { inputs: ['ts/**/*.ts'] });

task('css', () => jetpack.copyAsync('css', 'dist', { overwrite: true }), { inputs: ['css/*.*'] });

task('files', () => {
    jetpack.copyAsync('./README.md', 'dist/README.md', { overwrite: true });
    jetpack.copyAsync('./package.json', 'dist/package.json', { overwrite: true });
}, { inputs: ['./README.md', './package.json'] });

task('default', async () => {
    // @ts-ignore
    await tasks.clean();
    await Promise.all([
        // @ts-ignore
        tasks.ts(),
        // @ts-ignore
        tasks.css(),
        // @ts-ignore
        tasks.files()
    ]);
});

run();