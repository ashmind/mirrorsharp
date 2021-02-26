import fs from 'fs';
import { dirname, join as pathJoin } from 'path';
import { promisify } from 'util';
const sourceRootPath = dirname(dirname(require.resolve('../../../ts/mirrorsharp.ts')));

export default async function loadCSS(path: string): Promise<string> {
    const fullPath = pathJoin(sourceRootPath, path);
    return await promisify(fs.readFile)(fullPath, { encoding: 'utf-8' });
}