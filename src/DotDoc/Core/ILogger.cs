using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core
{
    public interface ILogger
    {
        void Info(string message);

        void Error(string message);
    }
}
