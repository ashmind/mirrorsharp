const jetpack = require('fs-jetpack');
const fg = require('fast-glob');
const ts = require('typescript');
const { task, tasks, run } = require('oldowan');

task('ts', async () => {
    const { options } = ts.getParsedCommandLineOfConfigFile('ts/tsconfig.json', undefined, ts.sys);
    const program = ts.createProgram(['ts/mirrorsharp.ts'], Object.assign(options, {
        module: ts.ModuleKind.CommonJS,
        noEmit: false,
        outDir: 'dist',
        declaration: true
    }));

    const emitResult = program.emit();
    const diagnostics = ts
        .getPreEmitDiagnostics(program)
        .concat(emitResult.diagnostics);

    for (const diagnostic of diagnostics) {
        if (diagnostic.file) {
            const { line, character } = diagnostic.file.getLineAndCharacterOfPosition(diagnostic.start);
            const message = ts.flattenDiagnosticMessageText(diagnostic.messageText, "\n");
            console.log(`${diagnostic.file.fileName} (${line + 1},${character + 1}): ${message}`);
        } else {
            console.log(ts.flattenDiagnosticMessageText(diagnostic.messageText, "\n"));
        }
    }
    if (emitResult.emitSkipped)
        throw new Error("TypeScript compilation failed.");

    // Add .js extension to all imports. Technically TypeScript already resolves
    // .js to .ts, but it's a hack.
    await Promise.all((await fg(['dist/**/*.js'])).map(async path => {
        const content = await jetpack.readAsync(path);
        const replaced = content.replace(/require\("(\.[^"]+)"\)/g, 'require("$1.js")');
        await jetpack.writeAsync(path, replaced);
    }));
}, { inputs: ['ts/**/*.ts'] });

task('css', () => jetpack.copyAsync('css', 'dist', { overwrite: true }), { inputs: ['css/*.*'] });

task('files', () => {
    jetpack.copyAsync('./README.md', 'dist/README.md', { overwrite: true });
    jetpack.copyAsync('./package.json', 'dist/package.json', { overwrite: true });
}, { inputs: ['./README.md', './package.json'] });

task('default', async () => Promise.all([
    tasks.js(),
    tasks.css(),
    tasks.files()
]));

run();