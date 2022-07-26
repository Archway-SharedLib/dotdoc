namespace DotDoc.Core.Write;

public interface IFsModelFactory
{
    IFileModel CreateFileModel(string fileName);
    
    IDirectoryModel CreateDirectoryModel(string path);
}

public interface IFileModel
{
}

public interface IDirectoryModel
{
}

public interface PhysicalFileModel : IFileModel
{
    
}

public interface PhysicalDirectoryModel : IDirectoryModel
{
    
}