using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using FSharp.Compiler.IO;
using Microsoft.FSharp.Core;
using MirrorSharp.FSharp.Advanced;
using MirrorSharp.Internal;

namespace MirrorSharp.FSharp.Internal {

    internal class CustomFileSystem : IFileSystem {
        private const string VirtualPathPrefix = "#mirrorsharp-virtual-fs";
        private const string VirtualTempPath = VirtualPathPrefix + "temp";

        private readonly ConcurrentDictionary<string, FSharpVirtualFile> _virtualFiles = new();
        private readonly ConcurrentDictionary<string, byte[]> _fileBytesCache = new();
        private readonly ConcurrentDictionary<string, bool> _fileExistsCache = new();
        private readonly ConcurrentDictionary<string, bool> _directoryExistsCache = new();

        public static CustomFileSystem Instance { get; } = new CustomFileSystem();

        private CustomFileSystem() {
            AssemblyLoader = new CustomAssemblyLoader();
        }

        public Stream OpenFileForReadShim(string filePath, FSharpOption<bool> useMemoryMappedFile, FSharpOption<bool> shouldShadowCopy) {
            if (GetVirtualFile(filePath) is {} virtualFile)
                return new NonDisposingStreamWrapper(virtualFile.GetStream());

            EnsureIsAssemblyFile(filePath);
            // For some reason, F# compiler requests this for same file many, many times.
            // Obviously, repeated IO is a bad idea.
            // Caching isn't great either, but will do for now.
            return new MemoryStream(_fileBytesCache.GetOrAdd(filePath, f => File.ReadAllBytes(f)));
        }

        public Stream OpenFileForWriteShim(string filePath, FSharpOption<FileMode> fileMode, FSharpOption<FileAccess> fileAccess, FSharpOption<FileShare> fileShare) {
            if (GetVirtualFile(filePath) is {} virtualFile)
                return new NonDisposingStreamWrapper(virtualFile.GetStream());

            throw new NotSupportedException();
        }

        public string GetFullPathShim(string fileName) {
            if (IsSpecialRangeFileName(fileName))
                return fileName;

            if (fileName.StartsWith(VirtualPathPrefix))
                return fileName;

            if (!Path.IsPathRooted(fileName))
                throw new NotSupportedException();
            return fileName;
        }

        public string GetFullFilePathInDirectoryShim(string dir, string fileName) {
            var path = IsPathRootedShim(fileName) ? fileName : Path.Combine(dir, fileName);
            return GetFullPathShim(path);
        }

        public string GetDirectoryNameShim(string path) {
            if (path == "")
                return ".";

            var dirName = Path.GetDirectoryName(path);
            if (dirName == null)
                return IsPathRootedShim(path) ? path : ".";

            return dirName == "" ? "." : dirName;
        }

        public DateTime GetLastWriteTimeShim(string fileName) {
            if (GetVirtualFile(fileName) is {} virtualFile)
                return virtualFile.LastWriteTime;

            EnsureIsAssemblyFile(fileName);
            // pretend all assemblies are ancient and unchanging
            // basically no support for assemblies dynamically changing during MirrorSharp session
            // which should be fine
            return DateTime.MinValue;
        }

        public DateTime GetCreationTimeShim(string path) {
            if (GetVirtualFile(path) is {} virtualFile)
                return DateTime.MinValue;

            EnsureIsAssemblyFile(path);
            // pretend all assemblies are ancient and unchanging
            // basically no support for assemblies dynamically changing during MirrorSharp session
            // which should be fine
            return DateTime.MinValue;
        }

        public void CopyShim(string src, string dest, bool overwrite) {
            throw new NotSupportedException();
        }

        public bool FileExistsShim(string fileName) {
            if (GetVirtualFile(fileName) != null)
                return true;

            if (!IsAssemblyFile(fileName) || fileName.StartsWith(VirtualTempPath, StringComparison.OrdinalIgnoreCase))
                return false;

            // For some reason, F# compiler requests this for same file many, many times.
            // Obviously, repeated IO is a bad idea.
            // Caching isn't great either, but will do for now.
            return _fileExistsCache.GetOrAdd(fileName, f => File.Exists(f));
        }

        public void FileDeleteShim(string fileName) {
            throw new NotSupportedException();
        }

        public DirectoryInfo DirectoryCreateShim(string path) {
            throw new NotSupportedException();
        }

        public bool DirectoryExistsShim(string path) {
            if (path.StartsWith(VirtualTempPath, StringComparison.OrdinalIgnoreCase))
                return false;

            if (path.StartsWith(VirtualPathPrefix, StringComparison.OrdinalIgnoreCase))
                return true;

            return _directoryExistsCache.GetOrAdd(path, f => Directory.Exists(f));
        }

        public void DirectoryDeleteShim(string path) {
            throw new NotSupportedException();
        }

        public IEnumerable<string> EnumerateFilesShim(string path, string pattern) {
            throw new NotSupportedException();
        }

        public IEnumerable<string> EnumerateDirectoriesShim(string path) {
            throw new NotSupportedException();
        }

        public string GetTempPathShim() {
            return VirtualTempPath;
        }

        public string NormalizePathShim(string path) {
            return GetFullPathShim(path);
        }

        public bool IsInvalidPathShim(string filename) {
            return filename.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
        }

        public bool IsPathRootedShim(string path) {
            if (path.StartsWith(VirtualPathPrefix))
                return true;

            return Path.IsPathRooted(path);
        }

        public bool IsStableFileHeuristic(string fileName) {
            // FSharp.Core's default implementation.
            var directory = Path.GetDirectoryName(fileName);
            return directory.Contains("Reference Assemblies/")
                || directory.Contains("Reference Assemblies\\")
                || directory.Contains("packages/")
                || directory.Contains("packages\\")
                || directory.Contains("lib/mono/");
        }

        public IAssemblyLoader AssemblyLoader { get; }

        private static void EnsureIsAssemblyFile(string fileName) {
            if (!IsAssemblyFile(fileName))
                throw new NotSupportedException();
        }

        private static bool IsAssemblyFile(string fileName) {
            return fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".optdata", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".sigdata", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSpecialRangeFileName(string fileName) {
            // File names used for ranges that are outside of specific source files
            // https://github.com/dotnet/fsharp/blob/dc81e22205550f0cedf4295b06c3a1e338c1cfa1/src/fsharp/range.fs#L226-L228
            return fileName is "unknown" or "startup" or "commandLineArgs"
                // https://github.com/dotnet/fsharp/blob/dc81e22205550f0cedf4295b06c3a1e338c1cfa1/src/fsharp/service/ServiceParsedInputOps.fs#L548
                or "";
        }

        public FSharpVirtualFile RegisterVirtualFile<TGetStreamContext>(
            Func<TGetStreamContext, MemoryStream> getStream,
            TGetStreamContext getStreamContext,
            string? fileName = null
        ) {
            Argument.NotNull(nameof(getStream), getStream);

            var path = Path.Combine(VirtualPathPrefix, Guid.NewGuid().ToString("D"));
            if (fileName != null)
                path = Path.Combine(path, fileName);

            var file = (FSharpVirtualFile?)null;
            try {
                file = new FSharpVirtualFile<TGetStreamContext>(path, getStream, getStreamContext, _virtualFiles);
                _virtualFiles.TryAdd(path, file);
                return file;
            }
            catch {
                file?.Dispose();
                throw;
            }
        }

        private FSharpVirtualFile? GetVirtualFile(string path)
            => _virtualFiles.TryGetValue(path, out var file)
             ? file
             : null;
    }
}