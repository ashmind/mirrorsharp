const path = require('path');
const fs = require('fs');
const jetpack = require('fs-jetpack');
const { task, tasks, run } = require('oldowan');

task('js', async () => {
    const structure = await jetpack.readAsync('./js/structure.js');
    const final = structure.replace(/\s*\/\/ include:\s*(\S+)\s*/g, (m, $1) => {
        const includePath = path.resolve(path.join('./js', $1));
        return '\r\n\r\n    ' + jetpack.read(includePath)
            .replace(/\/\* (?:globals|exported).+\*\//g, '')
            .trim()
            .replace(/\n/g, '\n    ');
    });
    await jetpack.writeAsync('dist/mirrorsharp.js', final);
}, { inputs: ['js/*.js'] });

task('css', () => jetpack.copyAsync('css', 'dist', { overwrite: true }), { inputs: ['css/*.*'] });
task('files', () => {
    jetpack.copyAsync('./README.md', 'dist/README.md', { overwrite: true });
    jetpack.copyAsync('./package.json', 'dist/package.json', { overwrite: true });
}, { inputs: ['./README.md', './package.json'] });
task('demoFix', async () => {
    await jetpack.dirAsync('./dist');
    if (await jetpack.existsAsync('./dist/node_modules'))
        return;
    // needed for local Demo to work
    fs.symlinkSync(path.resolve('./node_modules'), path.resolve('./dist/node_modules'), 'junction');
});

task('default', async () => Promise.all([
    tasks.js(),
    tasks.css(),
    tasks.files(),
    tasks.demoFix()
]));

run();