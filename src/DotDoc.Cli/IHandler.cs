using DotDoc.Core;

namespace DotDoc.Cli;

public interface IHandler
{
    public ValueTask Handle(ILogger logger);
}