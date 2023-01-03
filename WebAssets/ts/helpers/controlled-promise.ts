export type PromiseController<T> = {
    promise: Promise<T>;
    resolve: (value: T | PromiseLike<T>) => void;
    reject: (reason?: unknown) => void;
};

export const controlledPromise = <T>(): PromiseController<T> => {
    let captured: Omit<PromiseController<T>, 'promise'>;
    const promise = new Promise<T>((resolve, reject) => captured = { resolve, reject });

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    return { promise, ...captured! };
};