using System.Text;
using DotDoc.Core;
using DotDoc.Core.Read;
using DotDoc.Core.Write;
using DotDoc.Tests.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit.Abstractions;

namespace DotDoc.Tests.Core;

[UsesVerify]
public class CtorTest
{
    private readonly ILogger _logger;

    public CtorTest(ITestOutputHelper output)
    {
        _logger = new XUnitLogger(output);
    }
    
    [Fact]
    public async Task DefaultTest()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;

namespace Test;

/// <summary>NormalClassです。</summary>
public class NormalClass<T> {
    
    /// <summary> インスタンスを初期化します。</summary>
    public NormalClass(string name) {}

}

/// <summary>OverloadClassです。</summary>
public class OverloadClass<T> {

    /// <summary> インスタンスを初期化します。</summary>
    public OverloadClass() {}
    
    /// <summary> インスタンスを初期化します。</summary>
    public OverloadClass(string name) {}

    /// <summary> インスタンスを初期化します。</summary>
    public OverloadClass(string name, int age) {}
}
");
        var assems = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name.StartsWith("system", StringComparison.InvariantCultureIgnoreCase) || a.GetName().Name == "netstandard")
            .Select(a => MetadataReference.CreateFromFile(a.Location));
        
        var compilation = CSharpCompilation.Create("Assem", new[] { tree }, assems);
        var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(DotDocEngineOptions.Default("")), compilation));
        var outputText = new StringBuilder();
        var writer = new AdoWikiWriter(new[] { docItem }, DotDocEngineOptions.Default("test.sln"),
            new TestFsModel(outputText), _logger);
        await writer.WriteAsync();

        await Verify(outputText.ToString());
    }    
}