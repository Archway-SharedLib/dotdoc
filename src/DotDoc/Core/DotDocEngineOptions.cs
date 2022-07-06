using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core
{
    public class DotDocEngineOptions
    {
        public string? InputFileName { get; init; }

        public IEnumerable<string>? ExcludeIdPatterns { get; init; }

        public string? OutputDir { get; set; }
    }
}
