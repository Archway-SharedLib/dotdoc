using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core
{
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
            if (file.Extension != ".sln" && file.Extension != ".csproj")
            {
                logger.Error($"入力するファイルはソリューションファイル(.sln)もしくはC#プロジェクトファイル(.csproj)を指定してください。 {optins.InputFileName}");
                return false;
            }

            return true;
        }
    }
}
