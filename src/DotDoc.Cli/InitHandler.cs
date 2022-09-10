using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using DotDoc.Core;

namespace DotDoc.Cli;

public class InitHandler: IHandler
{
    public async ValueTask Handle(ILogger logger)
    {
        var currDir = Directory.GetCurrentDirectory();
        var slnFileName = Directory.GetFiles(currDir, "*.sln", SearchOption.AllDirectories).FirstOrDefault();
        var inputFile = slnFileName is not null ? Path.GetRelativePath(currDir, slnFileName) : string.Empty;
        if (!string.IsNullOrEmpty(inputFile))
        {
            logger.Trace($"Found {inputFile}.");
        }
        var options = new DotDocEngineOptions()
        {
            OutputDir = "./apidocs",
            InputFileName = inputFile,
            ExcludeIdPatterns = Array.Empty<string>()
        };
        var dotdocFile = Path.GetFullPath("./.dotdoc");
        using var stream = File.Open(dotdocFile, FileMode.Create);

        var serializerOptions = new JsonSerializerOptions()
        { 
            WriteIndented = true,
        };
        serializerOptions.Converters.Add(new JsonStringEnumConverter());
        await JsonSerializer.SerializeAsync(stream, options, serializerOptions);
        logger.Info($"Create dotdoc file. ${dotdocFile}");
    }
}