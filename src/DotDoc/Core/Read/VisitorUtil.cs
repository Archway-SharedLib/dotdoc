using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core.Read
{
    internal static class VisitorUtil
    {
        public static string GetSymbolId(IAssemblySymbol symbol) => $"A:{symbol.Name}";

        public static string GetSymbolId(ISymbol symbol) => symbol.GetDocumentationCommentId()!;
    }
}
