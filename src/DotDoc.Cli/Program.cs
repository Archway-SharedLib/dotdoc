// See https://aka.ms/new-console-template for more information

using DotDoc.Cli;

try
{
    var firstArg = args.FirstOrDefault();

    IHandler handler = (firstArg?.Equals("init", StringComparison.OrdinalIgnoreCase) == true) ? new InitHandler() :
        (firstArg?.Equals("run", StringComparison.OrdinalIgnoreCase) == true) ? new RunHandler() :
        new CommandsHandler();

    await handler.Handle();
}
catch (Exception e)
{
    Console.WriteLine(e);
}