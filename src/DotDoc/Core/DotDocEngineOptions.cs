using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core
{
    public class DotDocEngineOptions
    {
        public string OutputPath { get; init; } = "./output";

        public string? InputFileName { get; init; }
    }
}
