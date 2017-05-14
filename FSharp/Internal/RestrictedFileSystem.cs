using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.FSharp.Compiler.AbstractIL.Internal;

namespace MirrorSharp.FSharp.Internal {
    internal class RestrictedFileSystem : Library.Shim.IFileSystem {
        public Assembly AssemblyLoad(AssemblyName assemblyName) {
            return Assembly.Load(assemblyName);
        }

        public Assembly AssemblyLoadFrom(string fileName) {
            throw new NotSupportedException();
        }

        public void FileDelete(string fileName) {
            throw new NotSupportedException();
        }

        public Stream FileStreamCreateShim(string fileName) {
            throw new NotSupportedException();
        }

        public Stream FileStreamReadShim(string fileName) {
            throw new NotSupportedException();
        }

        public Stream FileStreamWriteExistingShim(string fileName) {
            throw new NotSupportedException();
        }

        public string GetFullPathShim(string fileName) {
            EnsureAllowed(fileName);
            if (!Path.IsPathRooted(fileName))
                throw new NotSupportedException();
            return fileName;
        }

        public DateTime GetLastWriteTimeShim(string fileName) {
            EnsureAllowed(fileName);
            return File.GetLastWriteTime(fileName);
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
            EnsureAllowed(fileName);
            return File.ReadAllBytes(fileName);
        }

        public bool SafeExists(string fileName) {
            if (fileName.EndsWith(".fs", StringComparison.OrdinalIgnoreCase))
                return false;
            EnsureAllowed(fileName);
            return File.Exists(fileName);
        }

        [AssertionMethod]
        private static void EnsureAllowed(string fileName) {
            if (!fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
             && !fileName.EndsWith(".optdata", StringComparison.OrdinalIgnoreCase)
             && !fileName.EndsWith(".sigdata", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException();
        }
    }
}