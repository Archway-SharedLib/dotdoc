using System.Text;
using DotDoc.Core;
using DotDoc.Core.Read;
using DotDoc.Core.Write;
using DotDoc.Tests.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit.Abstractions;
using Accessibility = DotDoc.Core.Accessibility;

namespace DotDoc.Tests.Core;

[UsesVerify]
public class ConstructorTest
{
    private readonly ILogger _logger;

    public ConstructorTest(ITestOutputHelper output)
    {
        _logger = new XUnitLogger(output);
    }

    [Fact]
    public async Task Single()
    {
        var md = await CreateMd(@"
using System;
// using System.Collections.Generic;

namespace Test;

public class Ctor
{
    /// <summary>コンストラクタです。</summary>
    public Ctor()
    {

    }
}
");
        await Verify(md);
    }

    [Fact]
    public async Task Multiple()
    {
        var md = await CreateMd(@"
using System;
// using System.Collections.Generic;

namespace Test;

public class Ctor
{
    /// <summary>コンストラクタです。</summary>
    public Ctor()
    {

    }
    /// <overloads>オーバーロードの説明です。</overloads>
    /// <summary>引数ありのコンストラクタです。</summary>
    public Ctor(string a)
    {

    }
}
");
        await Verify(md);
    }
    
    private async Task<string> CreateMd(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var assems = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name.StartsWith("system", StringComparison.InvariantCultureIgnoreCase) || a.GetName().Name == "netstandard")
            .Select(a => MetadataReference.CreateFromFile(a.Location));
        
        var compilation = CSharpCompilation.Create("Assem", new[] { tree }, assems);
        var options = new DotDocEngineOptions()
        {
            Accessibility = new[] { Accessibility.Public, Accessibility.Protected }
        };
        var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(options), compilation));
        var outputText = new StringBuilder();
        var writer = new AdoWikiWriter(new[] { docItem }, options,
            new TestFsModel(outputText), _logger);
        await writer.WriteAsync();
        return outputText.ToString();
    }
}