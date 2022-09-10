using DotDoc.Core;

namespace DotDoc.Cli;

public class CommandsHandler : IHandler
{
    public ValueTask Handle(ILogger _)
    {
        Console.WriteLine("commands");
        Console.WriteLine("  init   : Crete configuration file to current directory.");
        Console.WriteLine("  run    : Generate document.");
        return new ValueTask();
    }
}