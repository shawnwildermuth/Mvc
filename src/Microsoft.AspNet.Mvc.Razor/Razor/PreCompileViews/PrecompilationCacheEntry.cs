// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents an entry in the cache used by <see cref="RazorPreCompiler"/>.
    /// </summary>
    public class PrecompilationCacheEntry
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PrecompilationCacheEntry"/> that represents a successful
        /// parse.
        /// </summary>
        /// <param name="diagnostics">The <see cref="IReadOnlyList{Diagnostic}"/>s produced from parsing the Razor
        /// file.</param>
        public PrecompilationCacheEntry([NotNull] RazorFileInfo fileInfo,
                                             [NotNull] SyntaxTree syntaxTree)
        {
            FileInfo = fileInfo;
            SyntaxTree = syntaxTree;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PrecompilationCacheEntry"/> that represents a failure
        /// in Razor parsing.
        /// </summary>
        /// <param name="diagnostics">The <see cref="IReadOnlyList{Diagnostic}"/>s produced from parsing the Razor
        /// file.</param>
        public PrecompilationCacheEntry([NotNull] IReadOnlyList<Diagnostic> diagnostics)
        {
            Diagnostics = diagnostics;
        }

        /// <summary>
        /// Gets the <see cref="RazorFileInfo"/> associated with this cache entry instance.
        /// </summary>
        /// <remarks>
        /// This property is not null if <see cref="Success"/> is <c>true</c>.
        /// </remarks>
        public RazorFileInfo FileInfo { get; }

        /// <summary>
        /// Gets the <see cref="SyntaxTree"/> produced from parsing the generated contents of the
        /// file specified by <see cref="FileInfo"/>.
        /// </summary>
        /// <remarks>
        /// This property is not null if <see cref="Success"/> is <c>true</c>.
        /// </remarks>
        public SyntaxTree SyntaxTree { get; }

        /// <summary>
        /// Gets the <see cref="Diagnostic"/>s produced from parsing the generated contents of the file
        /// specified by <see cref="FileInfo"/>.
        /// </summary>
        /// <remarks>
        /// This property is not null if <see cref="Success"/> is <c>false</c>.
        /// </remarks>
        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        /// <summary>
        /// Gets a value that indicates that parsing was successful.
        /// </summary>
        public bool Success
        {
            get { return SyntaxTree != null; }
        }
    }
}