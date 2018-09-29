function kindsToClassName(kinds) {
    return kinds.map(function(kind) {
        return 'mirrorsharp-kind-' + kind;
    }).join(' ');
}

/* exported kindsToClassName */