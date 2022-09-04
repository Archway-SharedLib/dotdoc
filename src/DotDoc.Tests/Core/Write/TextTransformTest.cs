using System.Collections.Immutable;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
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
    public async Task SeeLangword()
    {
        var md = await CreateMd(@"
using System;
// using System.Collections.Generic;

namespace Test;

/// <summary><see langword=""string"" /> 参照です。<see langword=""RefClass"" /> RefClassです。<see langword=""List{T}""/></summary>
public class NormalClass {
}

/// <summary>参照です。<see langword=""Test.NormalClass"" /> RefClassです。<see langword=""String.IsNullOrEmpty"" /></summary>
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
    
    [Fact]
    public async Task Para()
    {
        var md = await CreateMd(@"
using System;
// using System.Collections.Generic;

namespace Test;

/// <summary>
/// パラム1
/// パラム2
/// <para>パラム3</para>
/// <para>パラム4</para>
/// パラム5
/// パラム6
/// </summary>
public class NormalClass {
}");
        await Verify(md);
    }
    
    [Fact]
    public async Task ParamRef()
    {
        var md = await CreateMd(@"
using System;
// using System.Collections.Generic;

namespace Test;

/// <summary><see cref=""string"" /> 参照です。<see cref=""RefClass"" /> RefClassです。<see cref=""List{T}"" /></summary>
public class NormalClass {
    
    /// <summary>Mメソッド。 <paramref name=""name"" /> パラメーター</summary>
    /// <param name=""name"">名前引数</param>
    /// <return><paramref name=""name"" />の値がそのまま返る</return>
    public string M(string name) => name;
}
");
        await Verify(md);
    }
    
    [Fact]
    public async Task TypeparamRef()
    {
        var md = await CreateMd(@"
using System;
// using System.Collections.Generic;

namespace Test;

/// <summary>型パラメーター<typeparamref name=""T"" />をもつ</summary>
/// <typeparam name=""T"">型</typeparam>
public class NormalClass<T> {
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