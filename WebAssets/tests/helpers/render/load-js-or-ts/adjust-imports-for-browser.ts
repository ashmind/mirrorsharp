import * as babel from '@babel/core';
import * as fs from 'fs';
import { dirname, relative, join as pathJoin } from 'path';

function rewriteImportPath(path: string, scriptFullPath: string) {
    let pathWithExtension = path;
    if (scriptFullPath.endsWith('.ts') && path.startsWith('.') && !path.endsWith('.ts'))
        pathWithExtension += '.ts';

    let resolvedFullPath;
    try {
        resolvedFullPath = require.resolve(pathWithExtension, { paths: [dirname(scriptFullPath)] });
    }
    catch (e) {
        console.warn(`Could not resolve import ${pathWithExtension} from ${scriptFullPath}:`, e as unknown);
        return `./unresolved-imports/${pathWithExtension}`;
    }

    resolvedFullPath = rewriteMainToModule(resolvedFullPath, pathWithExtension);

    const resolvedPath = relative(dirname(scriptFullPath), resolvedFullPath);
    const url = (resolvedPath.startsWith('.') ? '' : './') + resolvedPath.replace(/\\/g, '/');

    // console.log(`Rewriting ${path} to ${url} (imported in ${scriptFullPath}).`);
    return url;
}

function rewriteMainToModule(finalPath: string, initialPath: string) {
    const packageRootPath = finalPath.substring(0, finalPath.replace(/\\/g, '/').indexOf(initialPath) + initialPath.length);
    const packageJsonPath = pathJoin(packageRootPath, 'package.json');
    // eslint-disable-next-line no-sync
    if (!fs.existsSync(packageJsonPath))
        return finalPath;

    // eslint-disable-next-line @typescript-eslint/no-var-requires
    const packageJson = require(packageJsonPath) as { main: string; module?: string };
    const relativePath = relative(packageRootPath, finalPath).replace(/\\/g, '/');
    if (packageJson.main !== relativePath || !packageJson.module)
        return finalPath;

    return pathJoin(packageRootPath, packageJson.module);
}

export default function adjustImportsForBrowser(code: string, scriptFullPath: string) {
    const rewriteSource = (source: babel.types.StringLiteral) => {
        source.value = rewriteImportPath(source.value, scriptFullPath);
    };

    const result = babel.transform(code, {
        plugins: [
            {
                visitor: {
                    ImportDeclaration: ({ node: { source } }) => rewriteSource(source),
                    ExportAllDeclaration: ({ node: { source } }) => rewriteSource(source),
                    ExportNamedDeclaration: ({ node: { source } }) => {
                        if (!source)
                            return;
                        rewriteSource(source);
                    }
                }
            }
        ]
    });

    if (!result?.code)
        throw new Error(`Failed to transform ${scriptFullPath}`);

    return result.code;
}