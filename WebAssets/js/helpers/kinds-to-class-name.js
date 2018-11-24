/**
 * @param {ReadonlyArray<string>} kinds
 * @returns {string}
 * */
function kindsToClassName(kinds) {
    return 'mirrorsharp-has-kind ' + kinds.map(function(kind) {
        return 'mirrorsharp-kind-' + kind;
    }).join(' ');
}

/* exported kindsToClassName */