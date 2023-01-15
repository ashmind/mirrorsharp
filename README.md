## Overview

MirrorSharp is a reusable client-server code editor component built on [Roslyn](https://github.com/dotnet/roslyn) and [CodeMirror](https://codemirror.net/).

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

[![build](https://img.shields.io/github/actions/workflow/status/ashmind/mirrorsharp/dotnet.yml?style=flat-square)](https://github.com/ashmind/mirrorsharp/actions/workflows/dotnet.yml)

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

[![build](https://img.shields.io/github/actions/workflow/status/ashmind/mirrorsharp/web-assets.yml?style=flat-square)](https://github.com/ashmind/mirrorsharp/actions/workflows/web-assets.yml)
[![npm](https://img.shields.io/npm/v/mirrorsharp.svg?style=flat-square)](https://www.npmjs.com/package/mirrorsharp)

```
npm install mirrorsharp --save
```

#### Build

Since mirrorsharp JS files are not bundled, you'll need to use a bundler such as [Webpack](https://webpack.js.org), [Parcel](https://parceljs.org/) or [ESBuild](https://esbuild.github.io/).

You can see a Parcel example in AspNetCore.Demo.  

Note that mirrorsharp is written in TypeScript, and the package includes full TypeScript types.

**Note:** You need to manually require/import `mirrorsharp/mirrorsharp.css` into your bundle.

#### Usage
```javascript
import mirrorsharp from 'mirrorsharp';

mirrorsharp(container, { serviceUrl: 'wss://your_app_root/mirrorsharp' });
```

If you're not using HTTPS, you'll likely need `ws://` instead of `wss://`.

Note that the container is an actual HTML element, and not a CSS selector.

## API

TODO. In general the idea is that "it just works", however customization is a goal and some options are already available.

## Demos

You can check out the demos if you clone the repository locally.  
After cloning, run `ms setup` to build and prepare everything.

## Testing

TODO, but see MirrorSharp.Testing on NuGet.
