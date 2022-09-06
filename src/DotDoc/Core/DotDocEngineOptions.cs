using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotDoc.Core.Write;

namespace DotDoc.Core
{
    public class DotDocEngineOptions
    {
        private static readonly IEnumerable<Accessibility> _defaultAccessibility = new List<Accessibility>()
        {
            // Core.Accessibility.Private,
            Core.Accessibility.Protected,
            // Core.Accessibility.Internal,
            Core.Accessibility.Public,
            // Core.Accessibility.PrivateProtected,
            Core.Accessibility.ProtectedInternal
        };

        /// <summary>
        /// 読み取るソリューションまたはプロジェクトファイル名を指定します。
        /// </summary>
        public string? InputFileName { get; init; }

        /// <summary>
        /// 除外するIDのパターンを指定します。
        /// </summary>
        public IEnumerable<string>? ExcludeIdPatterns { get; init; }

        /// <summary>
        /// 出力するアクセス修飾子を指定します。
        /// </summary>
        public IEnumerable<Accessibility> Accessibility { get; init; } = _defaultAccessibility;

        /// <summary>
        /// 出力ディレクトリを指定します。
        /// </summary>
        public string? OutputDir { get; init; }

        /// <summary>
        /// 出力前に出力ディレクトリを削除するかどうかを指定します。
        /// </summary>
        public bool RemoveOutputDir { get; init; } = false;

        /// <summary>
        /// ログレベルを指定します。
        /// </summary>
        public LogLevel LogLevel { get; init; } = LogLevel.Info;

        /// <summary>
        /// 型メンバーをもたない空の名前空間を無視するかどうかを指定します。
        /// </summary>
        public bool IgnoreEmptyNamespace { get; init; } = true;

        /// <summary>
        /// AssemblyDocやNamespaceDocクラスを除外するかどうかを指定します。
        /// </summary>
        public bool ExcludeDocumentClass { get; set; } = true;
        
        // public Func<IFsModel> CreateFsModel { get; init; } = () => new PhysicalFsModel();

        public static DotDocEngineOptions Default(string inputFileName)
        {
            return new DotDocEngineOptions()
            {
                InputFileName = inputFileName,
                ExcludeIdPatterns = Enumerable.Empty<string>(),
                OutputDir = "./output"
            };
        }
    }
}
