using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.FSharp.Compiler.AbstractIL.Internal;
using MirrorSharp.FSharp.Advanced;

namespace MirrorSharp.FSharp.Internal {
    internal class CustomFileSystem : Library.Shim.IFileSystem {
        private readonly ConcurrentDictionary<string, IVirtualFileInternal> _virtualFiles = new ConcurrentDictionary<string, IVirtualFileInternal>();
        private readonly ConcurrentDictionary<string, byte[]> _fileBytesCache = new ConcurrentDictionary<string, byte[]>();
        private readonly ConcurrentDictionary<string, bool> _fileExistsCache = new ConcurrentDictionary<string, bool>();
        
        public static CustomFileSystem Instance { get; } = new CustomFileSystem();

        private CustomFileSystem() {
        }

        public Assembly AssemblyLoad(AssemblyName assemblyName) {
            return Assembly.Load(assemblyName);
        }

        public Assembly AssemblyLoadFrom(string fileName) {
            throw new NotSupportedException();
        }

        public void FileDelete(string fileName) {
            var virtualFile = GetVirtualFile(fileName);
            if (virtualFile != null) {
                virtualFile.Exists = false;
                return;
            }

            throw new NotSupportedException();
        }

        public Stream FileStreamCreateShim(string fileName) {
            var virtualFile = GetVirtualFile(fileName);
            if (virtualFile != null)
                return virtualFile.GetStream();

            throw new NotSupportedException();
        }

        public Stream FileStreamReadShim(string fileName) {
            var virtualFile = GetVirtualFile(fileName);
            if (virtualFile != null) {
                EnsureExists(virtualFile);
                return virtualFile.GetStream();
            }

            throw new NotSupportedException();
        }

        public Stream FileStreamWriteExistingShim(string fileName) {
            var virtualFile = GetVirtualFile(fileName);
            if (virtualFile != null)
                return virtualFile.GetStream();

            throw new NotSupportedException();
        }

        public string GetFullPathShim(string fileName) {
            if (GetVirtualFile(fileName) != null)
                return fileName;

            EnsureAllowed(fileName);
            if (!Path.IsPathRooted(fileName))
                throw new NotSupportedException();
            return fileName;
        }

        public DateTime GetLastWriteTimeShim(string fileName) {
            EnsureAllowed(fileName);
            // pretend all assemblies are ancient and unchanging
            // basically no support for assemblies dynamically changing during MirrorSharp session
            // which should be fine
            return DateTime.MinValue;
        }

        public string GetTempPathShim() {
            throw new NotSupportedException();
        }

        public bool IsInvalidPathShim(string filename) {
            return filename.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
        }

        public bool IsPathRootedShim(string path) {
            return Path.IsPathRooted(path);
        }

        public byte[] ReadAllBytesShim(string fileName) {
            var virtualFile = GetVirtualFile(fileName);
            if (virtualFile != null) {
                EnsureExists(virtualFile);
                return virtualFile.ReadAllBytes();
            }

            EnsureAllowed(fileName);
            // For some reason, F# compiler requests this for same file many, many times.
            // Obviously, repeated IO is a bad idea.
            // Caching isn't great either, but will do for now.
            return _fileBytesCache.GetOrAdd(fileName, f => File.ReadAllBytes(f));
        }

        public bool SafeExists(string fileName) {
            var virtualFile = GetVirtualFile(fileName);
            if (virtualFile != null)
                return virtualFile.Exists;

            if (fileName.EndsWith(".fs", StringComparison.OrdinalIgnoreCase))
                return false;
            EnsureAllowed(fileName);
            // For some reason, F# compiler requests this for same file many, many times.
            // Obviously, repeated IO is a bad idea.
            // Caching isn't great either, but will do for now.
            return _fileExistsCache.GetOrAdd(fileName, f => File.Exists(f));
        }

        [AssertionMethod]
        private static void EnsureAllowed(string fileName) {
            if (!fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
             && !fileName.EndsWith(".optdata", StringComparison.OrdinalIgnoreCase)
             && !fileName.EndsWith(".sigdata", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException($"File {fileName} has an extension that is not allowed by virtual file system.");
        }

        [AssertionMethod]
        private void EnsureExists(IVirtualFileInternal virtualFile) {
            if (!virtualFile.Exists)
                throw new FileNotFoundException($"Virtual file {virtualFile.Name} is marked as non-existent, and can only be written to.");
        }

        [NotNull]
        public FSharpVirtualFile RegisterVirtualFile([NotNull] MemoryStream stream, bool exists = true) {
            Argument.NotNull(nameof(stream), stream);

            var name = Guid.NewGuid().ToString("D");
            var file = (FSharpVirtualFile)null;
            try {
                file = new FSharpVirtualFile(name, stream, exists, _virtualFiles);
                _virtualFiles.TryAdd(name, file);
                return file;
            }
            catch {
                file?.Dispose();
                throw;
            }
        }
        
        public void RegisterVirtualFile([NotNull] IVirtualFileInternal virtualFile) {
            Argument.NotNull(nameof(virtualFile), virtualFile);
            
            try {
                _virtualFiles.TryAdd(virtualFile.Name, virtualFile);
            }
            catch {
                RemoveVirtualFile(virtualFile);
                throw;
            }
        }

        [CanBeNull]
        private IVirtualFileInternal GetVirtualFile([NotNull] string path) {
            return _virtualFiles.TryGetValue(Path.GetFileName(path), out var file) ? file : null;
        }

        public void RemoveVirtualFile([NotNull] IVirtualFileInternal virtualFile) {
            Argument.NotNull(nameof(virtualFile), virtualFile);
            _virtualFiles.TryRemove(virtualFile.Name, out IVirtualFileInternal _);
        }
    }
}