using System.Text;
using DotDoc.Core;
using DotDoc.Core.Read;
using DotDoc.Core.Write;
using DotDoc.Tests.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotDoc.Tests.Core;

[UsesVerify]
public class ClassVariationsTest
{
    [Fact]
    public async Task ClassVariations()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
namespace Test;

/// <summary>NormalClassです。</summary>
public class NormalClass {
}

/// <summary>StaticなClassです。</summary>
public static class StaticClass {
}

/// <summary>AbstractなClassです。</summary>
public abstract class AbstractClass {
}

/// <summary>SealedなClassです。</summary>
public sealed class SealedClass {
}
");
        var assems = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name.StartsWith("system", StringComparison.InvariantCultureIgnoreCase) || a.GetName().Name == "netstandard")
            .Select(a => MetadataReference.CreateFromFile(a.Location));
        
        var compilation = CSharpCompilation.Create("Test", new[] { tree }, assems);
        var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(DotDocEngineOptions.Default(""))));
        var outputText = new StringBuilder();
        var writer = new AdoWikiWriter(new[] { docItem }, DotDocEngineOptions.Default("test.sln"),
            new TestFsModel(outputText));
        await writer.WriteAsync();

        await Verify(outputText.ToString());
    }    
}