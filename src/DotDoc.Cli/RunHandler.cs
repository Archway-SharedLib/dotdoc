using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotDoc.Core;
using DotDoc.Core.Write;

namespace DotDoc.Cli;

public class RunHandler : IHandler
{
    public async ValueTask Handle(ILogger logger)
    {
        if (!File.Exists(".dotdoc"))
        {
            logger.Error("Cannot find `.dotdoc` file. Please run `dotdoc init` command.");
            return;
        }

        var serializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };
        serializerOptions.Converters.Add(new JsonStringEnumConverter());
        using var stream = File.OpenRead(".dotdoc");
        var options = await JsonSerializer.DeserializeAsync<DotDocEngineOptions>(stream, serializerOptions);
        if (options is null)
        {
            logger.Error("Cannot read `.dotdoc` file. Please check it.");
            return;
        }

        var engine = new DotDocEngine(new ConsoleLogger(options.LogLevel));
        var fsModel = new PhysicalFsModel();
        var docItems = (await engine.ReadAsync(options, fsModel)).ToList();
        await engine.WriteAsync(docItems, options, fsModel);
    }
}