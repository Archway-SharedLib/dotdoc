using System.Collections.Immutable;
using DotDoc.Core;
using DotDoc.Core.Write;
using FluentAssertions;

namespace DotDoc.Tests.Core.Write;

[UsesVerify]
public class TextTransformTest
{
    private ImmutableDictionary<string, DocItem> TestDocItems()
        => FlattenDocItems(ImmutableDictionary.CreateBuilder<string, DocItem>(), new List<AssemblyDocItem>()
        {
            new()
            {
                Id = "A:Assem1",
                DisplayName = "Assem1",
                Namespaces = new()
                {
                    new()
                    {
                        Id = "N:Ns1",
                        DisplayName = "Ns1",
                        Types = new()
                        {
                            new ClassDocItem()
                            {
                                Id = "T:Type1",
                                DisplayName = "Type1",
                            }
                        }
                    }
                }
            }
        }).ToImmutable();
    
    private ImmutableDictionary<string, DocItem>.Builder FlattenDocItems(ImmutableDictionary<string, DocItem>.Builder createBuilder, IEnumerable<DocItem> items)
    {
        foreach (var item in items.OrEmpty())
        {
            createBuilder.Add(item.Id, item);
            FlattenDocItems(createBuilder, item.Items);
        }

        return createBuilder;
    }
    
    private class TestFileSystemOperation: IFileSystemOperation
    {
        public string SafeFileOrDirectoryName(string sourceName) => sourceName;

        public string GetRelativeLink(DocItem baseItem, DocItem targetItem) => $"{targetItem.Id}.md";
    }

    [Fact]
    public void ToMdText_Seecref単体()
    {
        var transformer = new TextTransform(TestDocItems(), new TestFileSystemOperation());
        var transformed = transformer.ToMdText(new ClassDocItem(),new ClassDocItem(),_ => "<see cref=\"T:Type1\" />");
        transformed.Should().Be("[Type1](T:Type1.md)");
    }
    
    [Fact]
    public void ToMdText_Seecref後()
    {
        var transformer = new TextTransform(TestDocItems(), new TestFileSystemOperation());
        var transformed = transformer.ToMdText(new ClassDocItem(),new ClassDocItem(), _ => "<see cref=\"T:Type1\" />後ろに<dummy />別の文字");
        transformed.Should().Be("[Type1](T:Type1.md)後ろに\\<dummy /\\>別の文字");
    }
    
    [Fact]
    public void ToMdText_Seecref前()
    {
        var transformer = new TextTransform(TestDocItems(), new TestFileSystemOperation());
        var transformed = transformer.ToMdText(new ClassDocItem(),new ClassDocItem(), _ => "前に<dummy />別の文字<see cref=\"T:Type1\" />");
        transformed.Should().Be("前に\\<dummy /\\>別の文字[Type1](T:Type1.md)");
    }

    [Fact]
    public void ToMdText_Seecref前後()
    {
        var transformer = new TextTransform(TestDocItems(), new TestFileSystemOperation());
        var transformed = transformer.ToMdText(new ClassDocItem(),new ClassDocItem(), _ => "前に<dummy />別の文字<see cref=\"T:Type1\" />後ろに<dummy />別の文字");
        transformed.Should().Be("前に\\<dummy /\\>別の文字[Type1](T:Type1.md)後ろに\\<dummy /\\>別の文字");
    }
}