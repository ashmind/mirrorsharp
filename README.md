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

- **MirrorSharp.AspNetCore** on the server (MirrorSharp.Owin if .NET Framework)
- **mirrorsharp.js** — client library that provides the user interface

### Server

#### MirrorSharp.AspNetCore
[![NuGet](https://img.shields.io/nuget/v/MirrorSharp.AspNetCore.svg?style=flat-square)](https://www.nuget.org/packages/MirrorSharp.AspNetCore)

```powershell
Install-Package MirrorSharp.AspNetCore
```

If using Endpoint Routing (3.0+ only):
```csharp
app.UseEndpoints(endpoints => {
    // ...
    endpoints.MapMirrorSharp("/mirrorsharp");
});
```

If not using Endpoint Routing:
```csharp
app.MapMirrorSharp("/mirrosharp");
```

#### MirrorSharp.Owin
[![NuGet](https://img.shields.io/nuget/v/MirrorSharp.Owin.svg?style=flat-square)](https://www.nuget.org/packages/MirrorSharp.Owin)

```powershell
Install-Package MirrorSharp.Owin
```

In your `Startup`:
```csharp
app.MapMirrorSharp("/mirrosharp");
```

### Client
[![npm](https://img.shields.io/npm/v/mirrorsharp.svg?style=flat-square)](https://www.npmjs.com/package/mirrorsharp)

```
npm install mirrorsharp --save
```

#### CSS

If you are using LESS, CSS references can be done automatically by including `mirrorsharp/mirrorsharp.less`.

Otherwise, make sure to include the following:

1. `codemirror/lib/codemirror.css`
2. `codemirror/addon/lint/lint.css`
3. `codemirror/addon/hint/show-hint.css`
4. `codemirror-addon-infotip/dist/infotip.css`
5. `codemirror-addon-lint-fix/dist/lint-fix.css`
6. `mirrorsharp/mirrorsharp.css`

#### JS

Since mirrorsharp JS files are not bundled, you'll either need to use a bundler such as [Webpack](https://webpack.js.org) or [Parcel](https://parceljs.org/), or use `<script type="module">`.

AspNetCore.Demo project demonstrates use of Parcel.

Note that mirrorsharp is written in TypeScript and so the package includes full TypeScript types.

#### Usage
```javascript
import mirrorsharp from 'mirrorsharp';

mirrorsharp(textarea, { serviceUrl: 'wss://your_app_root/mirrorsharp' })
```

If you're not using HTTPS, you'll likely need `ws://` instead of `wss://`.

Note that `textarea` is an actual textarea element, and not a CSS selector or jQuery object.

## API

TODO. In general the idea is that "it just works", however customization is a goal and some options are already available.

## Demos

You can check out the demos if you clone the repository locally.  
After cloning, run `mirrorsharp setup` to build and prepare everything.

## Testing

TODO, but see MirrorSharp.Testing on NuGet.