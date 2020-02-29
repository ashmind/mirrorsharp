using System;
using System.ComponentModel;
using MirrorSharp.Owin.Internal;
using Owin;

namespace MirrorSharp.Owin {
    /// <summary>MirrorSharp-related extensions for the <see cref="IAppBuilder" />.</summary>
    public static class AppBuilderExtensions {
        /// <summary>Adds MirrorSharp middleware to the <see cref="IAppBuilder" />.</summary>
        /// <param name="app">The app builder.</param>
        /// <param name="options">The <see cref="MirrorSharpOptions" /> object used by the MirrorSharp middleware.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method will be removed in the next major version. Use app.MapMirrorSharp() instead.")]
        public static IAppBuilder UseMirrorSharp(this IAppBuilder app, MirrorSharpOptions? options = null) {
            Argument.NotNull(nameof(app), app);
            app.Use(typeof(Middleware), options ?? new MirrorSharpOptions());
            return app;
        }

        /// <summary>Maps MirrorSharp middleware to a certain path in the <see cref="IAppBuilder" />.</summary>
        /// <param name="app">The app builder.</param>
        /// <param name="path">Relative path to be used by MirrorSharp server, e.g. '/mirrorsharp'.</param>
        /// <param name="options">The <see cref="MirrorSharpOptions" /> object used by the MirrorSharp middleware.</param>
        [Obsolete("This method will be removed in the next major version. Use app.MapMirrorSharp() taking MirrorSharpServices instead.")]
        public static IAppBuilder MapMirrorSharp(this IAppBuilder app, string path, MirrorSharpOptions? options = null) {
            Argument.NotNull(nameof(app), app);
            Argument.NotNullOrEmpty(nameof(path), path);

            return app.Map(path, a => a.Use(typeof(Middleware), options ?? new MirrorSharpOptions(), new MirrorSharpServices {
                SetOptionsFromClient = options?.SetOptionsFromClient,
                SlowUpdate = options?.SlowUpdate,
                ExceptionLogger = options?.ExceptionLogger
            }));
        }

        /// <summary>Maps MirrorSharp middleware to a certain path in the <see cref="IAppBuilder" />.</summary>
        /// <param name="app">The app builder.</param>
        /// <param name="path">Relative path to be used by MirrorSharp server, e.g. '/mirrorsharp'.</param>
        /// <param name="options">The <see cref="MirrorSharpOptions" /> object used by the MirrorSharp middleware.</param>
        /// <param name="services">The <see cref="MirrorSharpServices" /> object used by the MirrorSharp middleware.</param>
        public static IAppBuilder MapMirrorSharp(this IAppBuilder app, string path, MirrorSharpOptions? options = null, MirrorSharpServices? services = null) {
            Argument.NotNull(nameof(app), app);
            Argument.NotNullOrEmpty(nameof(path), path);

            #pragma warning disable CS0618 // Type or member is obsolete
            if (options?.SetOptionsFromClient != null)
                throw new ArgumentException("SetOptionsFromClient must be passed through extensions and not options (options.SetOptionsFromClient must be null).");
            if (options?.SlowUpdate != null)
                throw new ArgumentException("SetOptionsFromClient must be passed through extensions and not options (options.SlowUpdate must be null).");
            if (options?.ExceptionLogger != null)
                throw new ArgumentException("SetOptionsFromClient must be passed through extensions and not options (options.ExceptionLogger must be null).");
            #pragma warning restore CS0618 // Type or member is obsolete

            return app.Map(path, a => a.Use(typeof(Middleware), options ?? new MirrorSharpOptions(), services ?? new MirrorSharpServices()));
        }
    }
}
