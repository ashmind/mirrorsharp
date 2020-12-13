import { dirname, join as pathJoin } from 'path';
import compileTestTypeScript from './compile-test-typescript';
import transformCommonJS from './transform-commonjs';
import { FSCache } from './fs-cache';
const sourceRootPath = dirname(dirname(require.resolve('../../../ts/mirrorsharp.ts')));
const testsRootPath = pathJoin(sourceRootPath, 'tests');

const cache = new FSCache(pathJoin(testsRootPath, '__render_cache__'));

function adjustImportsForBrowser(script: string) {
    return script.replace(/((?:from|import)\s*["'])([^"'.])/g, '$1/!/$2');
}

export default async function loadModuleByUrl(url: URL): Promise<string> {
    const path = url.pathname;

    const correctedPath = path.startsWith('/!/')
        ? path.replace(/^\/!\//, '') // node_modules and such
        : pathJoin(sourceRootPath, path + '.ts');

    const resolvedPath = require.resolve(correctedPath);
    const cached = await cache.get(resolvedPath);
    if (cached)
        return cached;

    console.log(`Loading ${path}`);
    if (resolvedPath.endsWith('.ts')) {
        const compiled = adjustImportsForBrowser(compileTestTypeScript(resolvedPath));
        await cache.set(resolvedPath, compiled);
        return compiled;
    }

    const content = adjustImportsForBrowser(await transformCommonJS(resolvedPath));
    await cache.set(resolvedPath, content);

    return content;
}