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
public class InheritDocTest
{
    private readonly ILogger _logger;

    public InheritDocTest(ITestOutputHelper output)
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

/// <summary>
/// <see cref=""ISource""/> の Summary です。
/// </summary>
public interface ISource{}

/// <inheritdoc />
public class Source : ISource{}

/// <inheritdoc />
public class BaseSource : ISource{}

/// <inheritdoc />
public class InheritBaseSource : BaseSource{}

/// <summary>
/// <see cref=""OverwriteSource""/> のコメントです。
/// </summary>
public class OverwriteSource : ISource{}

/// <inheritdoc />
public class InheritanceOverwriteSource : OverwriteSource{}

/// <summary>
/// <see cref=""ISecondSource""/> の Summary です。
/// </summary>
public interface ISecondSource{}

/// <inheritdoc />
public class MixSource:  ISource, ISecondSource{}

/// <inheritdoc cref=""ISecondSource"" />
public class MixSource2:  ISource, ISecondSource{}

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
