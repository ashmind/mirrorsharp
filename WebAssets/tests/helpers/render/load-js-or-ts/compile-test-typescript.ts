import * as ts from 'typescript';
import { resolve as pathResolve } from 'path';

const testRoot = pathResolve(`${__dirname}/../../../`);

const compiled = new Map<string, string>();

export default function compile(path: string): string {
    if (compiled.has(path))
        return compiled.get(path)!;

    // console.log(`Compiling ${path}`);

    const { config: { compilerOptions } } = ts.readConfigFile(`${testRoot}/tsconfig.json`, ts.sys.readFile);
    const { options, errors } = ts.convertCompilerOptionsFromJson(compilerOptions as unknown, testRoot);

    ensureNoErrors(errors, path);
    Object.assign(options, {
        noEmit: false,
        module: ts.ModuleKind.ESNext
    });

    const program = ts.createProgram([path], options);
    ensureNoErrors(ts.getPreEmitDiagnostics(program), path);

    // eslint-disable-next-line no-undefined
    const emitResult = program.emit(undefined, (path: string, data: string) => {
        const tsPath = pathResolve(path.replace(/\.js$/, '.ts'));
        // console.log(`Compiled ${tsPath}`);
        compiled.set(tsPath, data);
    });
    ensureNoErrors(emitResult.diagnostics, path);

    if (!compiled.has(path))
        throw new Error(`TypeScript compiler produced no outputs for ${path}.`);

    return compiled.get(path)!;
}

function ensureNoErrors(diagnostics: ReadonlyArray<ts.Diagnostic>, path: string) {
    if (diagnostics.length === 0)
        return;

    const errorsString = diagnostics.map(diagnostic => {
        if (diagnostic.file) {
            const { line, character } = diagnostic.file.getLineAndCharacterOfPosition(diagnostic.start!);
            const message = ts.flattenDiagnosticMessageText(diagnostic.messageText, '\n');
            return `${diagnostic.file.fileName} (${line + 1},${character + 1}): ${message}`;
        }
        else {
            return ts.flattenDiagnosticMessageText(diagnostic.messageText, '\n');
        }
    }).join('\n');

    throw new Error(`Failed to compile TypeScript file ${path}:\n${errorsString}`);
}