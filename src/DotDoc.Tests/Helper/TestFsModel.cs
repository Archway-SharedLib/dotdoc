using System.Text;
using DotDoc.Core.Write;

namespace DotDoc.Tests.Helper;

public class TestFsModel : IFsModel
{
    private readonly StringBuilder _stringBuilder;

    public TestFsModel(StringBuilder stringBuilder)
    {
        _stringBuilder = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
    }
    
    public IFileModel CreateFileModel(string fileName) => new TestFileModel(fileName, _stringBuilder);

    public IDirectoryModel CreateDirectoryModel(string path) => new TestDirectoryModel(path);

    public string PathJoin(string path1, string path2) => $"{path1}/{path2}";
    
}

public class TestFileModel : IFileModel
{
    private readonly string _fileName;
    private readonly StringBuilder _stringBuilder;

    public TestFileModel(string fileName, StringBuilder stringBuilder)
    {
        _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        _stringBuilder = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
    }

    public void WriteText(string text)
    {
        _stringBuilder.AppendLine();
        _stringBuilder.AppendLine("---------------------");
        _stringBuilder.AppendLine($"file name { _fileName}");
        _stringBuilder.AppendLine("---------------------");
        _stringBuilder.AppendLine();
        _stringBuilder.AppendLine(text);
    }

    public bool Exists() => false;

    public string GetExtension() => Path.GetExtension(_fileName);
}

public class TestDirectoryModel : IDirectoryModel
{
    private readonly string _directoryName;

    public TestDirectoryModel(string directoryName)
    {
        _directoryName = directoryName;
    }

    public string GetFullName() => _directoryName;

    public void CreateIfNotExists()
    {
    }
}


