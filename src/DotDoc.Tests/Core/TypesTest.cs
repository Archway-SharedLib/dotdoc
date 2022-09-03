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
public class TypesTest
{
    private readonly ILogger _logger;

    public TypesTest(ITestOutputHelper output)
    {
        _logger = new XUnitLogger(output);
    }
    
    [Fact]
    public async Task Types()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;

namespace Test;

/// <summary>Classです。</summary>
public class ClassType {
}

/// <summary>Enumです。</summary>
public enum EnumType {
}

/// <summary>Interfaceです。</summary>
public interface InterfaceType {
}

/// <summary>Structです。</summary>
public struct StructType {
}

/// <summary>Delegateです。</summary>
public delegate void DelegateType();

/// <summary>Recordです。</summary>
public record RecordType{
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