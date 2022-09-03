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
public class GenericTest
{
    private readonly ILogger _logger;

    public GenericTest(ITestOutputHelper output)
    {
        _logger = new XUnitLogger(output);
    }
    
    [Fact]
    public async Task DefaultTest()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
namespace Test;


/// <summary>
/// Genericクラスです。
/// </summary>
/// <typeparam name=""T"">Tの型です。</typeparam>
public class GenericClass<T> where T: new()
{
    /// <summary>
    /// T1です。
    /// </summary>
    /// <param name=""arg""><see cref=""T"" /></param>
    /// <typeparam name=""T1"">型です</typeparam>
    /// <returns>Tです</returns>
    public T GenericMethod<T1>(T arg) where T1: T => new T();

    /// <summary>
    /// T３つです。
    /// </summary>
    /// <param name=""arg""></param>
    /// <typeparam name=""T1""></typeparam>
    /// <typeparam name=""T2""></typeparam>
    /// <typeparam name=""T3""></typeparam>
    /// <returns></returns>
    public T3 GenericMethod<T1, T2, T3>(T arg) where T3 : new() => new T3();

}");
        var assems = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name.StartsWith("system", StringComparison.InvariantCultureIgnoreCase) || a.GetName().Name == "netstandard")
            .Select(a => MetadataReference.CreateFromFile(a.Location));
        
        var compilation = CSharpCompilation.Create("Assem", new[] { tree }, assems);
        var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(DotDocEngineOptions.Default(""))));
        var outputText = new StringBuilder();
        var writer = new AdoWikiWriter(new[] { docItem }, DotDocEngineOptions.Default("test.sln"),
            new TestFsModel(outputText), _logger);
        await writer.WriteAsync();

        await Verify(outputText.ToString());
    }    
    
    [Fact]
    public async Task GenericParamTest()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
using System.Collections.Generic;

namespace Test;

public class Dic
{
    public List<int> Method(Dictionary<string, List<int>> arg) => arg.Values.First();
}");
        var assems = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name.StartsWith("system", StringComparison.InvariantCultureIgnoreCase) || a.GetName().Name == "netstandard")
            .Select(a => MetadataReference.CreateFromFile(a.Location));
        
        var compilation = CSharpCompilation.Create("Assem", new[] { tree }, assems);
        var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(DotDocEngineOptions.Default(""))));
        var outputText = new StringBuilder();
        var writer = new AdoWikiWriter(new[] { docItem }, DotDocEngineOptions.Default("test.sln"),
            new TestFsModel(outputText), _logger);
        await writer.WriteAsync();

        await Verify(outputText.ToString());
    }    

}
