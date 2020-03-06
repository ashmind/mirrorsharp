export function kindsToClassName(kinds: ReadonlyArray<string>) {
    return 'mirrorsharp-has-kind '
         + kinds.map(kind => 'mirrorsharp-kind-' + kind).join(' ');
}