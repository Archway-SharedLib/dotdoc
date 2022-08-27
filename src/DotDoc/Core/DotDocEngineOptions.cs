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
            Core.Accessibility.Private,
            Core.Accessibility.Protected,
            Core.Accessibility.Internal,
            Core.Accessibility.Public,
            Core.Accessibility.PrivateProtected,
            Core.Accessibility.ProtectedInternal
        };

        public string? InputFileName { get; init; }

        public IEnumerable<string>? ExcludeIdPatterns { get; init; }

        public IEnumerable<Accessibility> Accessibility { get; init; } = _defaultAccessibility;

        public string? OutputDir { get; init; }

        public bool RemoveOutputDir { get; init; } = false;

        public LogLevel LogLevel { get; init; } = LogLevel.Info;
        
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
