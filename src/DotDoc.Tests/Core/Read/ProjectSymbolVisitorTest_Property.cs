using DotDoc.Core.Read;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Tests.Core.Read
{
    public class ProjectSymbolVisitorTest_Property
    {
        [Fact]
        public void Property()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
namespace Test;

public class TestClass {
    /// <summary>
    /// Getだけのプロパティです。
    /// </summary>
    public string GetOnly { get; }

    /// <summary>
    /// Setできるプロパティです。
    /// </summary>
    public int GetSet { get; set; }

    /// <summary>
    /// Initなプロパティです。
    /// </summary>
    public string GetInit { get; init; }

    /// <summary>
    /// Setだけのプロパティです。
    /// </summary>
    public string SetOnly { 
        set { }
    }

    /// <summary>
    /// Initだけのプロパティです。
    /// </summary>
    public string InitOnly
    {
        init { }
    }
}
");
            var compilation = CSharpCompilation.Create("Test", new[] { tree });
            var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(null)));
        }
    }
}
