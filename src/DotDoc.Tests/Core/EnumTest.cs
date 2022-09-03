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
public class EnumTest
{
    private readonly ILogger _logger;

    public EnumTest(ITestOutputHelper output)
    {
        _logger = new XUnitLogger(output);
    }
    
    [Fact]
    public async Task Enums()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;

namespace Test;

/// <summary>
/// 通常のEnumです。
/// </summary>
public enum NormalEnum
{
    /// <summary>
    /// ValueAです。
    /// </summary>
    ValueA,
    /// <summary>
    /// ValueBです。
    /// </summary>
    ValueB
}

/// <summary>
/// <see cref=""FlagsAttribute""/>付きのEnumです。
/// </summary>
[Flags]
public enum FlagsEnum
{
    /// <summary>
    /// 0です。
    /// </summary>
    None = 0,
    /// <summary>
    /// 1です
    /// </summary>
    ValueA = 1,
    /// <summary>
    /// 2です。
    /// </summary>
    ValueB = 2,
    /// <summary>
    /// 4です。
    /// </summary>
    ValueC = 4
}

/// <summary>
/// <see cref=""short""/> を元にしている Enum です。
/// </summary>
public enum ShortEnum : short {
    /// <summary>
    /// short 1です。
    /// </summary>
    ValueA = 1,
    /// <summary>
    /// short 2です。
    /// </summary>
    ValueB = 2
}
");
        var assems = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name.StartsWith("system", StringComparison.InvariantCultureIgnoreCase) ||
                        a.GetName().Name == "netstandard")
            .Select(a => MetadataReference.CreateFromFile(a.Location));

        var compilation = CSharpCompilation.Create("Test", new[] { tree }, assems);
        var docItem =
            compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(DotDocEngineOptions.Default(""))));
        var outputText = new StringBuilder();
        var writer = new AdoWikiWriter(new[] { docItem }, DotDocEngineOptions.Default("test.sln"),
            new TestFsModel(outputText), _logger);
        await writer.WriteAsync();

        await Verify(outputText.ToString());
    }
}