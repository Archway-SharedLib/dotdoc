namespace DotDoc.Cli;

public interface IHandler
{
    public ValueTask Handle();
}