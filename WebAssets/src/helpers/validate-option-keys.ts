export const validateOptionKeys = <T extends object, K extends ReadonlyArray<keyof T>>(
    options: T | undefined,
    keys: K & (keyof T extends K[number] ? unknown : `Missing option key: ${Exclude<keyof T, K[number] | symbol>}`),
    keyPrefix?: string
) => {
    if (!options)
        return;

    for (const key of Object.keys(options)) {
        if (!keys.includes(key as keyof T))
            throw new Error(`Unknown option: ${keyPrefix ? keyPrefix + '.' : ''}${key}`);
    }
};