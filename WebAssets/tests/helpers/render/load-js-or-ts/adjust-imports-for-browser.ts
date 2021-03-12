import * as babel from '@babel/core';
import { dirname, relative } from 'path';

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

    const resolvedPath = relative(dirname(scriptFullPath), resolvedFullPath);
    const url = (resolvedPath.startsWith('.') ? '' : './') + resolvedPath.replace(/\\/g, '/');

    console.log(`Rewriting ${path} to ${url} (imported in ${scriptFullPath}).`);
    return url;
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