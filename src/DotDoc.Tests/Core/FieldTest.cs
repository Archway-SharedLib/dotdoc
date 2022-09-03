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
public class FieldTest
{
    private readonly ILogger _logger;

    public FieldTest(ITestOutputHelper output)
    {
        _logger = new XUnitLogger(output);
    }
    
    [Fact]
    public async Task Fields()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;

namespace Test;

/// <summary>NormalClassです。</summary>
public class FieldClass {

    /// <summary>
    /// 文字列フィールドです。
    /// </summary>
    public string StringField;

    /// <summary>
    /// 文字列定数フィールドです。
    /// </summary>
    public const string ConstStringField = ""文字列"";

    /// <summary>
    /// 数値定数フィールドです。
    /// </summary>
    public const int ConstIntField = 123;

    /// <summary>
    /// 読み取り専用フィールドです。
    /// </summary>
    public readonly double ReadonlyDoubleField = 1.2;
    
    /// <summary>
    /// Staticなフィールドです。
    /// </summary>
    public static string StaticStringField;
    
    /// <summary>
    /// Staticな読み取り専用フィールドです。
    /// </summary>
    public static readonly double StaticReadonlyDoubleField = 1.2;

    /// <summary>
    /// Volatileなフィールドです。
    /// </summary>
    public volatile object VolatileFied;

    /// <summary>
    /// nullな定数フィールドです。
    /// </summary>
    public const FieldClass NullFiled = null;
}
");
        var assems = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name.StartsWith("system", StringComparison.InvariantCultureIgnoreCase) || a.GetName().Name == "netstandard")
            .Select(a => MetadataReference.CreateFromFile(a.Location));
        
        var compilation = CSharpCompilation.Create("Test", new[] { tree }, assems);
        var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(DotDocEngineOptions.Default("")), compilation));
        var outputText = new StringBuilder();
        var writer = new AdoWikiWriter(new[] { docItem }, DotDocEngineOptions.Default("test.sln"),
            new TestFsModel(outputText), _logger);
        await writer.WriteAsync();

        await Verify(outputText.ToString());
    }    
}