using DotDoc.Core.Read;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotDoc.Core;

namespace DotDoc.Tests.Core.Read
{
    public class ProjectSymbolVisitorTest_Delegate
    {
        [Fact]
        public void GetAccessibility() 
        {
            var tree = CSharpSyntaxTree.ParseText(@"
namespace Test;

public delegate string PublicDelegate(string value);

");
            var compilation = CSharpCompilation.Create("Test", new[] { tree });
            var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(DotDocEngineOptions.Default(""))));

        }
    }
    
    
}
