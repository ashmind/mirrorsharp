import { dirname, join as pathJoin, relative, resolve as pathResolve } from 'path';
import compileTestTypeScript from './compile-test-typescript';
import transformCommonJS from './transform-commonjs';
import { FSCache } from './fs-cache';

const sourceRootPath = dirname(dirname(require.resolve('../../../ts/mirrorsharp.ts')));
const testsRootPath = pathJoin(sourceRootPath, 'tests');

const cache = new FSCache(pathJoin(testsRootPath, '__render_cache__'));

function adjustImportsForBrowser(script: string, scriptFullPath: string) {
    return script.replace(/((?:from|import)\s*["'])([^"']+)/g, (_, prefix: string, path: string) => {
        let pathWithExtension = path;
        if (scriptFullPath.endsWith('.ts') && path.startsWith('.') && !path.endsWith('.ts'))
            pathWithExtension += '.ts';

        let resolvedFullPath;
        try {
            resolvedFullPath = require.resolve(pathWithExtension, { paths: [dirname(scriptFullPath)] });
        }
        catch (e) {
            console.warn(`Could not resolve import ${pathWithExtension} from ${scriptFullPath}:`, e as unknown);
            return prefix + `./unresolved-imports/${pathWithExtension}`;
        }

        const resolvedPath = relative(dirname(scriptFullPath), resolvedFullPath);
        const url = (resolvedPath.startsWith('.') ? '' : './') + resolvedPath.replace(/\\/g, '/');

        console.log(`Rewriting ${path} to ${url} (imported in ${scriptFullPath}).`);
        return prefix + url;
    });
}

export default async function loadJSOrTS(path: string): Promise<string> {
    const fullPath = pathResolve(sourceRootPath, path.replace(/^\//, ''));
    const cached = await cache.get(fullPath);
    if (cached)
        return cached;

    console.log(`Loading ${path} from ${fullPath}`);
    if (fullPath.endsWith('.ts')) {
        const compiled = adjustImportsForBrowser(compileTestTypeScript(fullPath), fullPath);
        await cache.set(fullPath, compiled);
        return compiled;
    }

    const content = adjustImportsForBrowser(await transformCommonJS(fullPath), fullPath);
    await cache.set(fullPath, content);

    return content;
}