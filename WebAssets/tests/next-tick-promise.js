module.exports = function nextTickPromise() {
    return new Promise(resolve => process.nextTick(resolve));
};