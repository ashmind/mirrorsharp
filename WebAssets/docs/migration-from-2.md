## Overview

Version `mirrorsharp-codemirror-6-preview` (future mirrorsharp 3.0+) includes a number of breaking changes when compared to `mirrorsharp` 2.

The goal of this document is to help with migration.

## Container

The most noticeable change is in the `mirrosharp()` contract, which now uses a container element instead of a `textarea`.

```typescript
// New:
// HTML: <div id="editor"></div>
mirrorsharp(document.getElementById('editor'));

// Old:
// HTML: <textarea id="editor"></textarea>
mirrorsharp(document.getElementById('editor'));
```

Note that this means you can't get/set the value of textarea directly, as you did with mirrorsharp 2.

To set the initial value, use the `text` option, e.g:
```typescript
mirrorsharp(container, { text: 'initial' });
```

To get or set the value after the editor is created, use the `getText()` and `setText()` on the editor instance:
```typescript
const ms = mirrorsharp(container);
ms.setText(ms.getText() + " // changed");
```

## Other changes

### Options

| Version 2              | CM 6 Preview         |
|------------------------|----------------------|
| `noInitialConnection`  | `disconnected`       |
| `initialServerOptions` | `serverOptions`      |
| `forCodeMirror`        | `codeMirror`         |
| `selfDebugEnabled`     | No longer available. |
| `on slowUpdateResult`  | Event property `x` renamed to `extensionData`. |
| `on connectionChange`  | Events `error` and `close` are no longer emitted. Event `loss` is emitted instead. |

Note that vesion 2 allowed unknown options, while newer versions will throw if any option is not known.

### Editor instance

| Version 2                            | CM 6 Preview        |
|--------------------------------------|---------------------|
| `getCodeMirror`                      | `getCodeMirrorView` |
| `destroy({ keepCodeMirror: true })` | No longer available. However more changes can be done without destroying, e.g. `setServiceUrl`. |

## CodeMirror

One of the major changes is that the library now uses CodeMirror 6.

Specific CodeMirror changes are out of scope of this document, but you can find the list in [CodeMirror 6 migration guide](https://codemirror.net/docs/migration).