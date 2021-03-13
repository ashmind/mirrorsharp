export default function controlledPromise<T>() {
    let captured: {
        resolve: (value?: T | PromiseLike<T>) => void;
        reject: (reason?: unknown) => void;
    };
    const promise = new Promise<T>((resolve, reject) => captured = { resolve, reject });

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    return { promise, ...captured! };
}