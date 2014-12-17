using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class RazorPreCompilationResult
    {
        public List<Diagnostic> ParseErrors { get; set; }

        public SyntaxTree SyntaxTree { get; set; }
    }
}