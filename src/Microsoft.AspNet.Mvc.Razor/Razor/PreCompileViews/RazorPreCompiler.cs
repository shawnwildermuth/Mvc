// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNet.FileSystems;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class RazorPreCompiler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileSystem _fileSystem;
        private readonly IMvcRazorHost _host;
        private readonly ICompilationCache _compilationCache;

        public RazorPreCompiler([NotNull] IServiceProvider designTimeServiceProvider) :
            this(designTimeServiceProvider,
                 designTimeServiceProvider.GetRequiredService<IMvcRazorHost>(),
                 designTimeServiceProvider.GetRequiredService<IOptions<RazorViewEngineOptions>>(),
                 designTimeServiceProvider.GetRequiredService<ICompilationCache>())
        {
        }

        public RazorPreCompiler([NotNull] IServiceProvider designTimeServiceProvider,
                                [NotNull] IMvcRazorHost host,
                                [NotNull] IOptions<RazorViewEngineOptions> optionsAccessor,
                                [NotNull] ICompilationCache compilationCache)
        {
            _serviceProvider = designTimeServiceProvider;
            _host = host;

            var appEnv = _serviceProvider.GetRequiredService<IApplicationEnvironment>();
            _fileSystem = optionsAccessor.Options.FileSystem;
            _compilationCache = compilationCache;
        }

        protected virtual string FileExtension { get; } = ".cshtml";

        public virtual void CompileViews([NotNull] IBeforeCompileContext context)
        {
            var descriptors = CreateCompilationDescriptors(context);

            if (descriptors.Count > 0)
            {
                var collectionGenerator = new RazorFileInfoCollectionGenerator(
                                                descriptors,
                                                SyntaxTreeGenerator.GetParseOptions(context.CSharpCompilation));

                var tree = collectionGenerator.GenerateCollection();
                context.CSharpCompilation = context.CSharpCompilation.AddSyntaxTrees(tree);
            }
        }

        protected virtual IReadOnlyList<RazorFileInfo> CreateCompilationDescriptors(
                                                            [NotNull] IBeforeCompileContext context)
        {
            var options = SyntaxTreeGenerator.GetParseOptions(context.CSharpCompilation);
            var list = new List<RazorFileInfo>();

            foreach (var info in GetFileInfosRecursive(currentPath: string.Empty))
            {
                var cachedValue = _compilationCache.Get(info.RelativePath, compilationContext =>
                {
                    compilationContext.Monitor(new FileWriteTimeCacheDependency(info.FileInfo.PhysicalPath));
                    return ParseView(info, options);
                });

                var razorPrecompilationResult = cachedValue as RazorPreCompilationResult;
                if (razorPrecompilationResult != null)
                {
                    if (razorPrecompilationResult.SyntaxTree != null)
                    {
                        context.CSharpCompilation = context.CSharpCompilation.AddSyntaxTrees(
                                                                razorPrecompilationResult.SyntaxTree);
                    }
                    else if (razorPrecompilationResult.ParseErrors != null)
                    {
                        foreach (var diagnostic in razorPrecompilationResult.ParseErrors)
                        {
                            context.Diagnostics.Add(diagnostic);
                        }
                    }
                }
            }

            return list;
        }

        private IEnumerable<RelativeFileInfo> GetFileInfosRecursive(string currentPath)
        {
            string path = currentPath;

            var fileInfos = _fileSystem.GetDirectoryContents(path);
            if (!fileInfos.Exists)
            {
                yield break;
            }

            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.IsDirectory)
                {
                    var subPath = Path.Combine(path, fileInfo.Name);

                    foreach (var info in GetFileInfosRecursive(subPath))
                    {
                        yield return info;
                    }
                }
                else if (Path.GetExtension(fileInfo.Name)
                         .Equals(FileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    var relativePath = Path.Combine(currentPath, fileInfo.Name);
                    var info = new RelativeFileInfo(fileInfo, relativePath);
                    yield return info;
                }
            }
        }

        protected virtual RazorPreCompilationResult ParseView([NotNull] RelativeFileInfo fileInfo,
                                                              [NotNull] CSharpParseOptions options)
        {
            var filePath = fileInfo.FileInfo.PhysicalPath;
            using (var stream = fileInfo.FileInfo.CreateReadStream())
            {
                var results = _host.GenerateCode(fileInfo.RelativePath, stream);

                if (!results.Success)
                {
                    var diagnostics = results.ParserErrors
                                         .Select(error => RazorErrorHelper.ToDiagnostics(error, filePath))
                                         .ToList();

                    return new RazorPreCompilationResult { ParseErrors = diagnostics };
                }
                
                var generatedCode = results.GeneratedCode;

                if (generatedCode != null)
                {
                    var syntaxTree = SyntaxTreeGenerator.Generate(generatedCode, fileInfo.FileInfo.PhysicalPath, options);
                    var fullTypeName = results.GetMainClassName(_host, syntaxTree);

                    if (fullTypeName != null)
                    {
                        return new RazorPreCompilationResult { SyntaxTree = syntaxTree };
                    }
                }
            }

            return null;
        }
    }
}

namespace Microsoft.Framework.Runtime
{
    [AssemblyNeutral]
    public interface IBeforeCompileContext
    {
        CSharpCompilation CSharpCompilation { get; set; }

        IList<ResourceDescription> Resources { get; }

        IList<Diagnostic> Diagnostics { get; }
    }
}
