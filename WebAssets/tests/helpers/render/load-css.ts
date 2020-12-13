import fs from 'fs';
import { dirname, join as pathJoin } from 'path';
import { promisify } from 'util';
const sourceRootPath = dirname(dirname(require.resolve('../../../ts/mirrorsharp.ts')));

export default async function loadCSS(url: URL): Promise<string> {
    const fullPath = pathJoin(sourceRootPath, url.pathname);
    return await promisify(fs.readFile)(fullPath, { encoding: 'utf-8' });
}