import jetpack from 'fs-jetpack';
import execa from 'execa';
import fg from 'fast-glob';
import oldowan from 'oldowan';
const { task, tasks, run } = oldowan;

task('ts', async () => {
    await execa.command('eslint . --max-warnings 0 --ext .js,.jsx,.ts,.tsx', {
        stdout: process.stdout,
        stderr: process.stderr
    });

    await execa.command('tsc --project ./ts/tsconfig.json --module ES2015 --noEmit false --outDir ./dist --declaration true', {
        stdout: process.stdout,
        stderr: process.stderr
    });

    // Add .js extension to all imports.
    // Technically TypeScript already resolves .js to .ts, but it's a hack.
    await Promise.all((await fg(['dist/**/*.js'])).map(async path => {
        const content = await jetpack.readAsync(path);
        const replaced = content.replace(/from '(\.[^']+)';/g, "from '$1.js';");
        await jetpack.writeAsync(path, replaced);
    }));
}, { inputs: ['ts/**/*.ts'] });

task('css', () => jetpack.copyAsync('css', 'dist', { overwrite: true }), { inputs: ['css/*.*'] });

task('files', () => {
    jetpack.copyAsync('./README.md', 'dist/README.md', { overwrite: true });
    jetpack.copyAsync('./package.json', 'dist/package.json', { overwrite: true });
}, { inputs: ['./README.md', './package.json'] });

task('default', async () => Promise.all([
    tasks.ts(),
    tasks.css(),
    tasks.files()
]));

run();