using System.Text;

namespace DotDoc.Core.Write;

public interface IFsModel
{
    IFileModel CreateFileModel(string fileName);
    
    IDirectoryModel CreateDirectoryModel(string path);

    string PathJoin(string path1, string path2);
}

public interface IFileModel
{
    void WriteText(string text);
    bool Exists();
    string GetExtension();
    string ReadAll();

    void Delete();
}

public interface IDirectoryModel
{
    string GetFullName();
    void CreateIfNotExists();
    void Delete();
}

public class PhysicalFileModel : IFileModel
{
    private readonly FileInfo _fileInfo;
    public PhysicalFileModel(FileInfo fileInfo)
    {
        _fileInfo = fileInfo;
    }

    public void WriteText(string text)
    {
        File.WriteAllText(_fileInfo.FullName, text, Encoding.UTF8);
    }

    public bool Exists() => _fileInfo.Exists;
    
    public string GetExtension() => _fileInfo.Extension;

    public string ReadAll()
    {
        using var sr = _fileInfo.OpenText(); ;
        return sr.ReadToEnd();
    }

    public void Delete()
    {
        if (!_fileInfo.Exists) return;
        _fileInfo.Delete();
    }
}

public class PhysicalFsModel : IFsModel
{
    public IFileModel CreateFileModel(string fileName) => new PhysicalFileModel(new FileInfo(fileName));

    public IDirectoryModel CreateDirectoryModel(string path) => new PhysicalDirectoryModel(new DirectoryInfo(path));
    public string PathJoin(string path1, string path2) => Path.Combine(path1, path2);
}

public class PhysicalDirectoryModel : IDirectoryModel
{
    private readonly DirectoryInfo _directoryInfo;

    public PhysicalDirectoryModel(DirectoryInfo directoryInfo)
    {
        _directoryInfo = directoryInfo;
    }

    public string GetFullName() => _directoryInfo.FullName;
    public void CreateIfNotExists()
    {
        if(!_directoryInfo.Exists) _directoryInfo.Create();
    }

    public void Delete()
    {
        if(_directoryInfo.Exists) _directoryInfo.Delete(true);
    }
}