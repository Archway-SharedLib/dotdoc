using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core
{
    public class DotDocEngineOptions
    {
        private static readonly IEnumerable<Accessibility> _defaultAccessibilities = new List<Accessibility>()
        {
            Accessibility.Private,
            Accessibility.Protected,
            Accessibility.Internal,
            Accessibility.Public,
            Accessibility.PrivateProtected,
            Accessibility.ProtectedInternal
        };

        public string? InputFileName { get; init; }

        public IEnumerable<string>? ExcludeIdPatterns { get; init; }

        public IEnumerable<Accessibility> Accessibilities { get; init; } = _defaultAccessibilities;

        public string? OutputDir { get; set; }

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
