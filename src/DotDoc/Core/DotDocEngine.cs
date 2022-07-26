using DotDoc.Core.Read;
using DotDoc.Core.Write;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace DotDoc.Core;

public class DotDocEngine
{
    private readonly ILogger _logger;

    public DotDocEngine(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<DocItem>> ReadAsync(DotDocEngineOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        var fsModel = options.CreateFsModel();
        if (!ValidateReadOptions(options, fsModel)) return Enumerable.Empty<DocItem>();

        MSBuildLocator.RegisterDefaults();
        var workspace = MSBuildWorkspace.Create();
        if (options.InputFileName!.EndsWith(Constants.SolutionFileExtension))
        {
            return await ReadSolutionFile(workspace, options);
        }
        else
        {
            var result = await ReadProjectFile(workspace, options);
            return result is null ? Enumerable.Empty<DocItem>() : new List<DocItem>() { result };
        }
    }

    public async Task WriteAsync(IEnumerable<DocItem> docItems, DotDocEngineOptions options)
    {
        if (docItems is null)
        {
            throw new ArgumentNullException(nameof(docItems));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        if (!ValidateWriteOptions(options)) return;

        var fsModelFactory = options.CreateFsModel();
        fsModelFactory.CreateDirectoryModel(options.OutputDir).CreateIfNotExists();
        
        var writer = new AdoWikiWriter(docItems, options, fsModelFactory);
        await writer.WriteAsync();
    }

    private async Task<IEnumerable<DocItem>> ReadSolutionFile(MSBuildWorkspace workspace, DotDocEngineOptions options)
    {
        var solution = await workspace.OpenSolutionAsync(options.InputFileName!);
        var results = new List<DocItem>();
        foreach (var proj in solution.Projects)
        {
            var result = await ReadProject(proj, options);
            if (result is not null)
            {
                results.Add(result);
            }
        }
        return results;
    }

    private async Task<DocItem?> ReadProjectFile(MSBuildWorkspace workspace, DotDocEngineOptions options)
    {
        var proj = await workspace.OpenProjectAsync(options.InputFileName!);
        return await ReadProject(proj, options);
    }

    private async Task<DocItem?> ReadProject(Project proj, DotDocEngineOptions options)
    {
        var compilation = await proj.GetCompilationAsync();
        if (compilation is null) return null;

        return compilation.Assembly.Accept(new ProjectSymbolsVisitor(new DefaultFilter(options)));
    }

    private bool ValidateReadOptions(DotDocEngineOptions optins, IFsModel fsModel)
    {
        if (string.IsNullOrWhiteSpace(optins.InputFileName))
        {
            _logger.Error($"入力パスが指定されていません。 {optins.InputFileName}");
            return false;
        }

        var file = fsModel.CreateFileModel(optins.InputFileName);
        // var file = new FileInfo(optins.InputFileName);
        if (!file.Exists())
        {
            _logger.Error($"入力するファイルが見つかりません。 {optins.InputFileName}");
            return false;
        }
        if (file.GetExtension() != Constants.SolutionFileExtension && file.GetExtension() != Constants.CsProjectFileExtension)
        {
            _logger.Error($"入力するファイルはソリューションファイル({Constants.SolutionFileExtension})" +
                         $"もしくはC#プロジェクトファイル({Constants.CsProjectFileExtension})を指定してください。 {optins.InputFileName}");
            return false;
        }

        return true;
    }

    private bool ValidateWriteOptions(DotDocEngineOptions optins)
    {
        if (string.IsNullOrWhiteSpace(optins.OutputDir))
        {
            _logger.Error($"出力ディレクトリが指定されていません。 {optins.OutputDir}");
            return false;
        }
        return true;
    }
}

