import { dirname, join as pathJoin, resolve as pathResolve } from 'path';
import compileTestTypeScript from './load-js-or-ts/compile-test-typescript';
import transformCommonJS from './load-js-or-ts/transform-commonjs';
import adjustImportsForBrowser from './load-js-or-ts/adjust-imports-for-browser';
import { FSCache } from './fs-cache';

const sourceRootPath = dirname(dirname(require.resolve('../../../ts/mirrorsharp.ts')));
const testsRootPath = pathJoin(sourceRootPath, 'tests');

const cache = new FSCache(pathJoin(testsRootPath, '.render-cache'));

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