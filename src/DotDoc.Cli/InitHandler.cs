using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using DotDoc.Core;

namespace DotDoc.Cli;

public class InitHandler: IHandler
{
    public async ValueTask Handle()
    {
        var currDir = Directory.GetCurrentDirectory();
        var slnFileName = Directory.GetFiles(currDir, "*.sln", SearchOption.AllDirectories).FirstOrDefault();
        var inputFile = slnFileName is not null ? Path.GetRelativePath(currDir, slnFileName) : string.Empty;
        var options = new DotDocEngineOptions()
        {
            OutputDir = "./apidocs",
            Accessibility = new[]
            {
                Accessibility.Public,
                Accessibility.Protected
            },
            InputFileName = inputFile,
            ExcludeIdPatterns = Array.Empty<string>()
        };
        using var stream = File.Open("./.dotdoc", FileMode.Create);

        var serializerOptions = new JsonSerializerOptions()
        { 
            WriteIndented = true,
        };
        serializerOptions.Converters.Add(new JsonStringEnumConverter());
        await JsonSerializer.SerializeAsync(stream, options, serializerOptions);
    }
}