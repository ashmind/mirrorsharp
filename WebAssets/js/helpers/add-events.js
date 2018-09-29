function addEvents(target, handlers) {
    for (var key in handlers) {
        target.on(key, handlers[key]);
    }
    return function() {
        // eslint-disable-next-line no-shadow
        for (var key in handlers) {
            target.off(key, handlers[key]);
        }
    };
}

/* exported addEvents */