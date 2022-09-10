// See https://aka.ms/new-console-template for more information

using DotDoc;
using DotDoc.Cli;
using DotDoc.Core;

try
{
    var firstArg = args.FirstOrDefault();

    IHandler handler = (firstArg?.Equals("init", StringComparison.OrdinalIgnoreCase) == true) ? new InitHandler() :
        (firstArg?.Equals("run", StringComparison.OrdinalIgnoreCase) == true) ? new RunHandler() :
        new CommandsHandler();

    var logger = new ConsoleLogger(LogLevel.All);
    await handler.Handle(logger);
}
catch (Exception e)
{
    Console.WriteLine(e);
}