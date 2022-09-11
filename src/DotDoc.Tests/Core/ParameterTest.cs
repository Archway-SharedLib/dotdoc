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
public class ParameterTest
{
    private readonly ILogger _logger;

    public ParameterTest(ITestOutputHelper output)
    {
        _logger = new XUnitLogger(output);
    }

    [Fact]
    public async Task Default()
    {
        var md = await CreateMd(@"
using System;
// using System.Collections.Generic;

namespace Test;

public static class ParameterCheck
{
    public static void M1(this int n) 
    {
    }

    public static void M2(params int[] values)
    {
    }

    public static void M3(in int param = 1)
    {
    }

    public static void M4(out int param)
    {
        param = 1;
    }
    
    public static void M5(ref int param)
    {
        param = 1;
    }

    public static void M6(int v = 1)
    {
    }
    
    public static void M7(int? v)
    {
    }

    /// <param name=""p"">
    /// パラメーターのコメントです。
    /// <para>パラグラフです。</para>
    /// リストです。
    /// <list type=""number"">
    /// <item><description>１つ目です。</description></item>
    /// <item><description>２つ目です。</description></item>
    /// <item><description>３つ目です。</description></item>
    /// <item><description>４つ目です。</description></item>
    /// </list>
    /// </param>
    public static void MultilineParameterComment(string p)
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