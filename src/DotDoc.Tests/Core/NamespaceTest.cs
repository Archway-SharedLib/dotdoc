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
public class NamespaceTest
{
    private readonly ILogger _logger;

    public NamespaceTest(ITestOutputHelper output)
    {
        _logger = new XUnitLogger(output);
    }
    
    [Fact]
    public async Task Namespace()
    {
        var tree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections.Generic;

namespace Test{
    /// <summary>名前空間ドキュメントです</summary>
    internal class NamespaceDoc{
    }
}

namespace Test.Test2{
    public class Test3{
    }

    /// <summary>Test.Test2の名前空間ドキュメントです</summary>
    internal class NamespaceDoc{
    }
}
");
        var assems = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name.StartsWith("system", StringComparison.InvariantCultureIgnoreCase) || a.GetName().Name == "netstandard")
            .Select(a => MetadataReference.CreateFromFile(a.Location));
        
        var compilation = CSharpCompilation.Create("Test", new[] { tree }, assems);

        var options = new DotDocEngineOptions()
        {
            IgnoreEmptyNamespace = true,
            Accessibility = new[] { Accessibility.Public, Accessibility.Protected }
        };
        
        var docItem = compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(options), compilation));
        var outputText = new StringBuilder();
        var writer = new AdoWikiWriter(new[] { docItem }, options,
            new TestFsModel(outputText), _logger);
        await writer.WriteAsync();

        await Verify(outputText.ToString());
    }    
}