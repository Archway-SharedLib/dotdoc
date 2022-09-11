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
        var md = await CreateMd(@"
namespace Test;


/// <summary>
/// Genericクラスです。
/// </summary>
/// <typeparam name=""T"">Tの型です。</typeparam>
public class GenericClass<T> where T: new()
{
    /// <overloads>
    /// GenericMethodです。
    /// </overloads>
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
        await Verify(md);
    }    
    
    [Fact]
    public async Task GenericParamTest()
    {
        var md = await CreateMd(@"
using System.Collections.Generic;

namespace Test;

public class Dic
{
    public List<int> Method(Dictionary<string, List<int>> arg) => arg.Values.First();
}");
        await Verify(md);
    }
    
    [Fact]
    public async Task ConstraintTest()
    {
        var md = await CreateMd(@"
using System.Collections.Generic;

namespace Test;

public class Dic<T, T2> where T : IList<string>, class? where T2: T, new()
{
}");
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
            Accessibility = new[] { Accessibility.Public, Accessibility.Protected },
            OutputDir = "./output"
        };
        var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(options), compilation));
        var outputText = new StringBuilder();
        var writer = new AdoWikiWriter(new[] { docItem }, options,
            new TestFsModel(outputText), _logger);
        await writer.WriteAsync();
        return outputText.ToString();
    }
}
