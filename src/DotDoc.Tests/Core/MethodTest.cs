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
public class MethodTest
{
    private readonly ILogger _logger;

    public MethodTest(ITestOutputHelper output)
    {
        _logger = new XUnitLogger(output);
    }
    
    [Fact]
    public async Task Methods()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
namespace Test;

public abstract class BaseMethodVariation
{
    /// <summary>
    /// Baseメソッドです
    /// </summary>
    public void BaseMethod()
    {
    }
    
    /// <summary>
    /// バーチャルです。
    /// </summary>
    public virtual void VirtualMethod(){}

    /// <summary>
    /// abstractです。
    /// </summary>
    public abstract void AbstractMethod();
}

public class MethodVariation : BaseMethodVariation
{
    /// <summary>
    /// リターンなし引数なし
    /// </summary>
    public void ReturnVoidNoParam()
    {
    }

    /// <summary>
    /// リターン文字列引数なし
    /// </summary>
    /// <returns>文字列の <see cref=""string""/> を返します。</returns>
    public string ReturnStringNoParam() => string.Empty;

    /// <summary>
    /// リターンなし引数2
    /// </summary>
    /// <param name=""param1""> 文字列の <see cref=""string""/> です </param>
    /// <param name=""param2""> 数字の <see cref=""int""/> です</param>
    public void ReturnVoidWithParam(string param1, int param2)
    {
    }

    /// <summary>
    /// リターン数字引数2
    /// </summary>
    /// <param name=""param1""> 文字列の <see cref=""string""/> です </param>
    /// <param name=""param2""> 数字の <see cref=""int""/> です</param>
    /// <returns>数字の <see cref=""int""/> を返します。</returns>
    public int ReturnIntWithParam(string param1, int param2) => param2;

    /// <inheritdoc />
    public override void AbstractMethod()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override void VirtualMethod()
    {
        base.VirtualMethod();
    }
    
    public new void BaseMethod(){}
    
}");

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
    
    [Fact]
    public async Task GenericMethods()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
namespace Test;

public class GenericMethod
{
    /// <summary>
    /// 一つのメソッドです
    /// </summary>
    public void SingleMethod<T>()
    {
    }
    
    /// <summary>
    /// ２つのメソッドです
    /// </summary>
    /// <typeparam name=""TArg"">TArgです</typeparam>
    /// <typeparam name=""TVal"">TValです</typeparam>
    public string DoubleMethod<TArg, TVal>() => string.Empty();

    /// <summary>
    /// 引数に使います。
    /// </summary>
    public TVal DoubleMethodWithParam<TArg, TVal>(TArg arg)
    {
        return default(TVal);
    }

    /// <summary>
    /// リターン数字引数2
    /// </summary>
    /// <param name=""param1""> 文字列の <see cref=""string""/> です </param>
    /// <param name=""param2""> 数字の <see cref=""int""/> です</param>
    /// <returns>数字の <see cref=""int""/> を返します。</returns>
    public int ReturnIntWithParam(in string param1, ref int param2, out int param3)
    {
        param3 = 1;
        return 1;
    }

    public ref readonly int ReturnRefReadOnly(in int v)
    {
        ref readonly var result = ref v;
        return ref result;
    }
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
    
    [Fact]
    public async Task Overload()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
namespace Test;

public class MethodOverload
{
    
    /// <summary>
    /// リターンなし引数なし
    /// </summary>
    public void OverloadMethod()
    {
    }

    /// <summary>
    /// 文字引数
    /// </summary>
    /// <param name=""str"">Strです。</param>
    public void OverloadMethod(string str)
    {
    }

    /// <summary>
    /// 数字引数
    /// </summary>
    /// <param name=""num"">Numです。</param>
    public void OverloadMethod(int num)
    {
    }

    /// <summary>
    /// 文字数字引数
    /// </summary>
    /// <param name=""num"">Numです。</param>
    /// <param name=""str"">Strです。</param>
    /// <returns>strです</returns>
    public string OverloadMethod(int num, string str) => str;

    /// <summary>
    /// 文字T引数
    /// </summary>
    /// <param name=""num"">Numです。</param>
    /// <param name=""str"">Strです。</param>
    /// <typeparam name=""T"">Tです</typeparam>
    /// <returns>strです</returns>
    public string OverloadMethod<T>(T num, string str) => str;

    /// <summary>
    /// T1T2引数
    /// </summary>
    /// <param name=""num"">Numです。</param>
    /// <param name=""str"">Strです。</param>
    /// <typeparam name=""T"">Tです</typeparam>
    /// <typeparam name=""T2"">T2です</typeparam>
    /// <returns>T2です</returns>
    public T2 OverloadMethod<T, T2>(T num, T2 str) => str;
}");

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
    
    [Fact]
    public async Task ReturnTuple()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;

namespace Test;

public class ReturnTuple
{
    public (T2, string) OverloadMethod<T, T2>(T num, T2 str) => (str, string.Empty());
}");

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
    
    [Fact]
    public async Task InheritDoc()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
namespace Test;

public interface BaseInterface
{
    /// <summary>
    /// <see cref=""BaseInterface""/> の <see cref=""IM""/> のサマリです。
    /// </summary>
    /// <param name=""a"">BaseInterface a 引数です</param>
    /// <returns><see cref=""int""/> </returns>
    /// <returns><see cref=""int""/> です。</returns>
    int IM(DateTime a);
}

public class BaseType
{
    /// <summary>
    /// <see cref=""BaseType""/> の <see cref=""M""/> のサマリです。
    /// </summary>
    /// <param name=""a"">BaseType a 引数です</param>
    /// <param name=""b"">Baseype b 引数です</param>
    /// <returns><see cref=""BaseType""/> の <see cref=""M""/> の戻り値です。</returns>
    public virtual string M(string a, int b) => a;
}

public class InheritDoc: BaseType
{
    /// <inheritdoc />
    public override string M(string a, int b) => a;
}

public class NoInheritDoc
{
    /// <inheritdoc cref=""BaseType.M""/>
    public string M(string a, int b) => a;
}

public class BaseImplDoc: BaseInterface
{
    /// <inheritdoc />
    public int IM(DateTime a) => a.Millisecond;
}");

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
