using DotDoc.Core.Read;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Tests.Core.Read
{
    public class ProjectSymbolVisitorTest_Accessibility
    {
        [Fact]
        public void GetAccessibility() 
        {
            var tree = CSharpSyntaxTree.ParseText(@"
namespace Test;

public class PublicClass {}

private class PrivateClass {}

protected class ProtectedClass {}

internal class InternalClass {}

protected internal class ProtectedInternalClass {}

private protected class PrivateProtectedClass {}

");
            var compilation = CSharpCompilation.Create("Test", new[] { tree });
            var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(null)));

        }
    }
    
    
}
