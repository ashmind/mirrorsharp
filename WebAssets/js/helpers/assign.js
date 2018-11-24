/** @type {<T>(target: T, ...sources: Array<T>) => T} */
// eslint-disable-next-line es/no-object-assign
const assign = Object.assign || function (target) {
    for (var i = 1; i < arguments.length; i++) {
        var source = arguments[i];
        for (var key in source) {
            // @ts-ignore
            target[key] = source[key];
        }
    }
    return target;
};

/* exported assign */