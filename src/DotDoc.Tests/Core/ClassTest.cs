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
public class ClassTest
{
    private readonly ILogger _logger;

    public ClassTest(ITestOutputHelper output)
    {
        _logger = new XUnitLogger(output);
    }
    
    [Fact]
    public async Task Classes()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
namespace Test;

/// <summary>NormalClassです。</summary>
/// <remarks>
/// このクラスは特に何も処理をおこないません。
/// <para>
/// パラグラフです。
/// </para>
/// </remarks>
/// <example>
/// 使用例です。
/// <code>
/// new NormalClass();
/// </code>
/// 呼び出しです。
/// <code>
/// var a = ""aaa"";
/// </code>
/// </example>
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

/// <summary><see cref=""AbstractClass"" /> を継承したクラス。</summary>
public class InheritClass: AbstractClass {
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