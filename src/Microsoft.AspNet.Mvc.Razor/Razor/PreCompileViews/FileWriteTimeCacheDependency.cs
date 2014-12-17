// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Mvc.Razor
{
    internal class FileWriteTimeCacheDependency : ICompilationCacheDependency
    {
        private readonly string _path;
        private readonly DateTime _lastWriteTime;

        public FileWriteTimeCacheDependency(string path)
        {
            _path = path;
            _lastWriteTime = File.GetLastWriteTime(path);
        }

        public bool HasChanged
        {
            get
            {
                return _lastWriteTime < File.GetLastWriteTime(_path);
            }
        }

        public override string ToString()
        {
            return _path;
        }

        public override bool Equals(object obj)
        {
            var token = obj as FileWriteTimeCacheDependency;
            return token != null && token._path.Equals(_path, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return _path.GetHashCode();
        }
    }
}