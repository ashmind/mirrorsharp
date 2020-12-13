import fs from 'fs';
import { promisify } from 'util';
import path from 'path';

export class FSCache {
    readonly #basePath: string;
    #basePathConfirmed = false;
    readonly #memory = new Map<string, { dateMs: number; value: string }>();

    constructor(basePath: string) {
        this.#basePath = basePath;
    }

    async set(sourcePath: string, value: string) {
        if (!this.#basePathConfirmed && !(await promisify(fs.exists)(this.#basePath))) {
            await promisify(fs.mkdir)(this.#basePath);
            this.#basePathConfirmed = true;
        }

        const cachePath = this.#getCachePath(sourcePath);

        await promisify(fs.writeFile)(cachePath, value);
        this.#memory.set(sourcePath, {
            dateMs: await this.#getLastModifiedMs(cachePath),
            value
        });
    }

    async get(sourcePath: string) {
        const memory = this.#memory.get(sourcePath);
        if (memory) {
            const sourceDateMs = await this.#getLastModifiedMs(sourcePath);
            /*if (memory.dateMs >= sourceDateMs) {
                console.log(`Using memory-cached value for '${sourcePath}'.`);
                return memory.value;
            }
            else {
                console.log(`Memory cache of '${sourcePath}' is oudated (${memory.dateMs} < ${sourceDateMs})`);
                return null;
            }*/
            return (memory.dateMs >= sourceDateMs) ? memory.value : null;
        }

        const cachePath = this.#getCachePath(sourcePath);
        if (!(await promisify(fs.exists)(cachePath)))
            return null;

        const [sourceDateMs, cacheDateMs, cacheValue] = await Promise.all([
            this.#getLastModifiedMs(sourcePath),
            this.#getLastModifiedMs(cachePath),
            promisify(fs.readFile)(cachePath, { encoding: 'utf-8' })
        ]);
        this.#memory.set(sourcePath, {
            dateMs: cacheDateMs,
            value: cacheValue
        });

        /*if (cacheDateMs >= sourceDateMs) {
            console.log(`Using file-cached value for '${sourcePath}'.`);
            return cacheValue;
        }
        else {
            console.log(`File cache of '${sourcePath}' is oudated (${cacheDateMs} < ${sourceDateMs})`);
            return null;
        }*/
        return (cacheDateMs >= sourceDateMs) ? cacheValue : null;
    }

    #getLastModifiedMs = async (path: string) => (await promisify(fs.stat)(path)).mtimeMs;

    #getCachePath = (sourcePath: string) => {
        return path.join(this.#basePath, sourcePath.replace(/[/\\:.]/g, '_'));
    };
}