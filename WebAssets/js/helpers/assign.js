var assign = Object.assign || function (target) {
    for (var i = 1; i < arguments.length; i++) {
        var source = arguments[i];
        for (var key of source) {
            target[key] = source[key];
        }
    }
    return target;
};

/* exported assign */