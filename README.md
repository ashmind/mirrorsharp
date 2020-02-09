## Overview

[![AppVeyor branch](https://img.shields.io/appveyor/ci/ashmind/mirrorsharp/master.svg?style=flat-square)](https://ci.appveyor.com/project/ashmind/mirrorsharp) 
[![AppVeyor tests](https://img.shields.io/appveyor/tests/ashmind/mirrorsharp.svg?style=flat-square)](https://ci.appveyor.com/project/ashmind/mirrorsharp/build/tests)

MirrorSharp is a code editor `<textarea>` built on Roslyn and [CodeMirror](https://codemirror.net/).

### Features
#### Code completion
![Code completion](📄readme/code-completion.png)

#### Signature help
![Signature help](📄readme/signature-help.png)

#### Quick fixes
![Quick fixes](📄readme/quick-fixes.png)

#### Diagnostics
![Diagnostics](📄readme/diagnostics.png)

#### Quick info
![Quick info](📄readme/infotips.png)
  
## Usage

You'll need the following:

- **MirrorSharp.Owin** on the server (.NET Core is planned, but not supported yet)
- **mirrorsharp.js** — client library that provides the user interface

### Server

#### MirrorSharp.AspNetCore
[![NuGet](https://img.shields.io/nuget/v/MirrorSharp.AspNetCore.svg?style=flat-square)](https://www.nuget.org/packages/MirrorSharp.AspNetCore)

NuGet: `Install-Package MirrorSharp.AspNetCore`  
Once installed, call `app.UseMirrorSharp()` in your `Startup`.

#### MirrorSharp.Owin
[![NuGet](https://img.shields.io/nuget/v/MirrorSharp.Owin.svg?style=flat-square)](https://www.nuget.org/packages/MirrorSharp.Owin)

NuGet: `Install-Package MirrorSharp.Owin`  
Once installed, call `app.UseMirrorSharp()` in your OWIN startup.

### Client
[![npm](https://img.shields.io/npm/v/mirrorsharp.svg?style=flat-square)](https://www.npmjs.com/package/mirrorsharp)

NPM: `npm install mirrorsharp --save`

#### CSS
If you are using LESS, CSS references can be done automatically by including `mirrorsharp/mirrorsharp.less`.  
Otherwise, make sure to include the following:

1. codemirror/lib/codemirror.css
2. codemirror/addon/lint/lint.css
3. codemirror/addon/hint/show-hint.css
4. codemirror-addon-infotip/dist/infotip.css
5. codemirror-addon-lint-fix/dist/lint-fix.css
6. mirrorsharp/mirrorsharp.css

#### JS
JS can be done automatically since mirrorsharp has proper requires. Otherwise:

1. codemirror/lib/codemirror.js
2. codemirror/mode/clike/clike.js
3. codemirror/addon/lint/lint.js
4. codemirror/addon/hint/show-hint.js
5. codemirror-addon-infotip/dist/infotip.js
6. codemirror-addon-lint-fix/dist/lint-fix.js
7. mirrorsharp/mirrorsharp.js

#### Usage
Once referenced, you can do the following:
```javascript
mirrorsharp(textarea, { serviceUrl: 'wss://your_app_root/mirrorsharp' })
```
If you're not using HTTPS, you'll likely need `ws://` instead of `wss://`.

Note that `textarea` is an actual textarea element, and not a CSS selector or jQuery object.

## API

TODO. In general the idea is that "it just works", however customization is a goal and some options are already available.

## Demos

You can check out the demos if you clone the repository locally.  
After cloning, run `.\mirrorsharp build` to build and prepare everything.

## Testing

TODO, but see MirrorSharp.Testing on NuGet.
