using DotDoc.Core.Models;

namespace DotDoc.Core.Write;

public interface IFileSystemOperation
{
    string SafeFileOrDirectoryName(string sourceName);

    string GetRelativeLink(IDocItem baseItem, IDocItem targetItem);
}