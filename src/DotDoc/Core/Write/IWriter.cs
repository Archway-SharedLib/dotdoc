using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core.Write
{
    internal interface IWriter
    {
        Task WriteAsync();
    }
}
