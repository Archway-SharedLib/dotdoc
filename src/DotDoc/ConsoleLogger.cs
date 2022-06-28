using DotDoc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc
{
    public class ConsoleLogger : ILogger
    {
        public void Error(string message)
        {
            Console.Error.WriteLine(message);
        }

        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }
}
