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
    public class ProjectSymbolVisitorTest_Property
    {
        [Fact]
        public void Property()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
namespace Test;

public abstract class BaseTestClass {
    /// <summary>
    /// protected なベースプロパティです、
    /// </summary>
    protected string BaseProtected { get; }
    
    /// <summary>
    /// virtual なベースプロパティです。
    /// </summary>
    protected virtual string BaseVirtual { get; }
    
    /// <summary>
    /// abstract なベースプロパティです。
    /// </summary>
    protected abstract string BaseAbstract { get; init; }
}

public class TestClass: BaseTestClass {
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

    public static string StaticProp { get; set; }

    protected new string BaseProtected { get; }

    /// <inheritdoc />
    protected override string BaseVirtual { get; }
    
    /// <inheritdoc />
    protected override string BaseAbstract { get; init; }
}
");
            var compilation = CSharpCompilation.Create("Test", new[] { tree });
            var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(DotDocEngineOptions.Default(""))));
        }
    }
}
