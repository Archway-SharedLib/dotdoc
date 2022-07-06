using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core.Read
{
    public interface IFilter
    {
        bool Exclude(ISymbol symbol, string id);
    }
}
