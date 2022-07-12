using DotDoc.Core.Read;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Tests.Core.Read
{
    public class ProjectSymbolVisitorTest_Parameter
    {
        [Fact]
        public void GetMethodParameter() 
        {
            var tree = CSharpSyntaxTree.ParseText(@"
namespace Test;

public class TestClass {

    /// <summary>
    /// テストを取得します。
    /// </summary>
    /// <param name=""foo"">Fooパラメーターです</param>
    /// <param name=""bar"">Barパラメーターです。</param>
    /// <returns>戻り値です。</returns>
    public string GetTest(string foo, int bar) {
        return foo;
    }
}
");
            var compilation = CSharpCompilation.Create("Test", new[] { tree });
            var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(null)));

        }
    }
}
