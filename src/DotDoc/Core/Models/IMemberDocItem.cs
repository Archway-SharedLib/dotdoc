namespace DotDoc.Core.Models;

public interface IMemberDocItem : IDocItem
{
    string? AssemblyId { get; }

    string? NamespaceId { get; }

    string? TypeId { get; }
}