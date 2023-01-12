export const omit = <T extends object, K extends keyof T>(value: T, keys: ReadonlyArray<K>) => {
    return Object.fromEntries(Object.entries(value).filter(
        ([key]) => !keys.includes(key as K)
    ));
};