namespace DotDoc.Core.Write;

public interface IFileSystemOperation
{
    string SafeFileOrDirectoryName(string sourceName);

    string GetRelativeLink(DocItem baseItem, DocItem targetItem);
}