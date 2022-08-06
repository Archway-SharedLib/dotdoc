using System.Reflection;
using System.Text;
using DotDoc.Core;
using DotDoc.Core.Read;
using DotDoc.Core.Write;
using DotDoc.Tests.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotDoc.Tests.Core;

[UsesVerify]
public class PropertyTest
{
    [Fact]
    public async Task Properties()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
namespace Test;

/// <summary>
/// プロパティを検証するためのabstractクラスです。
/// </summary>
public abstract class BasePropertyVariation
{
    /// <summary>
    /// protected なベースプロパティです、
    /// </summary>
    protected string BaseProtected { get; }
    
    /// <summary>
    /// virtual なベースプロパティです。
    /// </summary>
    protected virtual string BaseVirtual { get; }
    
    /// <summary>
    /// abstract なベースプロパティです。
    /// </summary>
    protected abstract string BaseAbstract { get; init; }
}

public class PropertyVariation : BasePropertyVariation
{
    
    /// <summary>
    /// 読み書きプロパティです。
    /// </summary>
    /// <value><see cref=""string"" /></value>
    public string ReadWrite { get; set; }
    
    /// <summary>
    /// 読みとり専用プロパティです。
    /// </summary>
    public string Read { get; }
    
    /// <summary>
    /// 書き込み専用プロパティです。
    /// </summary>
    public string Write {
        set { }
    }

    /// <summary>
    /// イニット専用プロパティです。
    /// </summary>
    public string InitWrite
    {
        init { }
    }
    
    /// <summary>
    /// 読みイニットプロパティです。
    /// </summary>
    public string ReadInit { get; init; }

    /// <summary>
    /// 静的プロパティです。
    /// </summary>
    public static string StaticProp { get; set; }
    
    /// <summary>
    /// newプロパティです。
    /// </summary>
    protected new string BaseProtected { get; }

    /// <inheritdoc />
    protected override string BaseVirtual { get; }
    
    /// <inheritdoc />
    protected override string BaseAbstract { get; init; }
}");

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