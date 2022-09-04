using System.Collections.Immutable;
using System.Text;
using DotDoc.Core;
using DotDoc.Core.Read;
using DotDoc.Core.Write;
using DotDoc.Tests.Helper;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit.Abstractions;

namespace DotDoc.Tests.Core.Write;

[UsesVerify]
public class TextTransform
{
    private readonly ILogger _logger;

    public TextTransform(ITestOutputHelper output)
    {
        _logger = new XUnitLogger(output);
    }
    
    [Fact]
    public async Task SeeCref()
    {
        var md = await CreateMd(@"
using System;
// using System.Collections.Generic;

namespace Test;

/// <summary><see cref=""string"" /> 参照です。<see cref=""RefClass"" /> RefClassです。<see cref=""List{T}""/></summary>
public class NormalClass {
}

/// <summary>参照です。<see cref=""Test.NormalClass"" /> RefClassです。<see cref=""String.IsNullOrEmpty"" /></summary>
public class RefClass{
}
");
        await Verify(md);
    }
    
    [Fact]
    public async Task C()
    {
        var md = await CreateMd(@"
using System;
// using System.Collections.Generic;

namespace Test;

/// <summary><c>null</c> です。 <c>foo = ""bar""</c> です。 <c>これは？</c>からの場合のテスト <c></c>です。</summary>
public class NormalClass {
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
        var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(DotDocEngineOptions.Default("")), compilation));
        var outputText = new StringBuilder();
        var writer = new AdoWikiWriter(new[] { docItem }, DotDocEngineOptions.Default("test.sln"),
            new TestFsModel(outputText), _logger);
        await writer.WriteAsync();
        return outputText.ToString();
    }
}