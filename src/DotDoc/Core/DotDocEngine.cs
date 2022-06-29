using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace DotDoc.Core;

public class DotDocEngine
{
    private readonly ILogger logger;

    public DotDocEngine(ILogger logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Execute(DotDocEngineOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        if (!ValidateOptions(options)) return;
        
        MSBuildLocator.RegisterDefaults();
        var workspace = MSBuildWorkspace.Create();
        if (options.InputFileName.EndsWith(Constants.SolutionFileExtension))
        {
            await ReadSolutionFile(workspace, options.InputFileName);
        }
        else
        {
            await ReadProjectFile(workspace, options.InputFileName);
        }
    }

    private async Task ReadSolutionFile(MSBuildWorkspace workspace, string solutionFileName)
    {
        var solution = await workspace.OpenSolutionAsync(solutionFileName);
        foreach (var proj in solution.Projects)
        {
            await ReadProject(proj);
        }
        
    }
    
    private async Task ReadProjectFile(MSBuildWorkspace workspace, string projectFileName)
    {
        var proj = await workspace.OpenProjectAsync(projectFileName);
        await ReadProject(proj);
    }

    private async Task ReadProject(Project proj)
    {
        var compilation = await proj.GetCompilationAsync();
        compilation.Assembly.Accept(new ProjectSymbolsVisitor());
    }
    

    private bool ValidateOptions(DotDocEngineOptions optins)
    {
        if(string.IsNullOrWhiteSpace(optins.OutputPath))
        {
            logger.Error($"出力パスが指定されていません。 {optins.OutputPath}");
            return false;
        }
        if (string.IsNullOrWhiteSpace(optins.InputFileName))
        {
            logger.Error($"入力パスが指定されていません。 {optins.InputFileName}");
            return false;
        }
        var file = new FileInfo(optins.InputFileName);
        if (!file.Exists)
        {
            logger.Error($"入力するファイルが見つかりません。 {optins.InputFileName}");
            return false;
        }
        if (file.Extension != Constants.SolutionFileExtension && file.Extension != Constants.CsProjectFileExtension)
        {
            logger.Error($"入力するファイルはソリューションファイル({Constants.SolutionFileExtension})" +
                         $"もしくはC#プロジェクトファイル({Constants.CsProjectFileExtension})を指定してください。 {optins.InputFileName}");
            return false;
        }

        return true;
    }
}

