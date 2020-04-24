import jetpack from 'fs-jetpack';
import fg from 'fast-glob';
import { transformFileAsync } from '@babel/core';
import { task, exec, build } from 'oldowan';
import convertPrivateFields from './build/babel-plugin-convert-private-fields-to-symbols';

const clean = task('clean', async () => {
    const paths = await fg(['dist/**/*.*', '!dist/node_modules/**/*.*']);
    await Promise.all(paths.map(jetpack.removeAsync));
});

const ts = task('ts', async () => {
    await exec('eslint ./ts --max-warnings 0 --ext .js,.jsx,.ts,.tsx');
    await exec('tsc --project ./ts/tsconfig.json --module ES2015 --noEmit false --outDir ./dist --declaration true');

    await Promise.all((await fg(['dist/**/*.js'])).map(async path => {
        const { code: transformed } = (await transformFileAsync(path, {
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
        }))!;

        await jetpack.writeAsync(path, transformed!);
    }));
}, { inputs: ['ts/**/*.ts'] });

const css = task('css', () => jetpack.copyAsync('css', 'dist', { overwrite: true }), { inputs: ['css/*.*'] });

const files = task('files', () => {
    jetpack.copyAsync('./README.md', 'dist/README.md', { overwrite: true });
    jetpack.copyAsync('./package.json', 'dist/package.json', { overwrite: true });
}, { inputs: ['./README.md', './package.json'] });

task('default', async () => {
    await clean();
    await Promise.all([
        ts(),
        css(),
        files()
    ]);
});

build();