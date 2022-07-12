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
        public static string GetSymbolId(IAssemblySymbol symbol) => $"{IdPrefix.Assembly}:{symbol.Name}";

        public static string GetSymbolId(ISymbol symbol) => symbol.GetDocumentationCommentId()!;

        public static string GetSymbolId(IParameterSymbol symbol) => $"{IdPrefix.Parameter}:{symbol.ContainingSymbol.ToDisplayString()}+{symbol.Name}";
    }
}
