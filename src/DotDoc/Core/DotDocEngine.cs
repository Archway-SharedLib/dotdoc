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

    public async Task<IEnumerable<IDocItem>> ReadAsync(DotDocEngineOptions options, IFsModel fsModel)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (fsModel is null) throw new ArgumentNullException(nameof(fsModel));
        
        if (!ValidateReadOptions(options, fsModel)) return Enumerable.Empty<IDocItem>();

        MSBuildLocator.RegisterDefaults();
        var workspace = MSBuildWorkspace.Create();
        if (options.InputFileName!.EndsWith(Constants.SolutionFileExtension))
        {
            return await ReadSolutionFile(workspace, options);
        }
        else
        {
            var result = await ReadProjectFile(workspace, options);
            return result is null ? Enumerable.Empty<IDocItem>() : new List<IDocItem>() { result };
        }
    }

    public async Task WriteAsync(IEnumerable<IDocItem> docItems, DotDocEngineOptions options, IFsModel fsModel)
    {
        if (docItems is null)
        {
            throw new ArgumentNullException(nameof(docItems));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (fsModel is null) throw new ArgumentNullException(nameof(fsModel));
        
        if (!ValidateWriteOptions(options)) return;

        _logger.Info($"write to {options.OutputDir}");

        var dirModel = fsModel.CreateDirectoryModel(options.OutputDir);
        if (options.RemoveOutputDir)
        {
            dirModel.Delete();
        }
        
        fsModel.CreateDirectoryModel(options.OutputDir).CreateIfNotExists();
        
        var writer = new AdoWikiWriter(docItems, options, fsModel, _logger);
        await writer.WriteAsync();
    }

    private async Task<IEnumerable<IDocItem>> ReadSolutionFile(MSBuildWorkspace workspace, DotDocEngineOptions options)
    {
        var solution = await workspace.OpenSolutionAsync(options.InputFileName!);
        _logger.Info($"Read solution: {solution.Id}");

        var results = new List<IDocItem>();
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

    private async Task<IDocItem?> ReadProjectFile(MSBuildWorkspace workspace, DotDocEngineOptions options)
    {
        
        var proj = await workspace.OpenProjectAsync(options.InputFileName!);
        return await ReadProject(proj, options);
    }

    private async Task<IDocItem?> ReadProject(Project proj, DotDocEngineOptions options)
    {
        _logger.Info($"Read project: {proj.Name}");

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

