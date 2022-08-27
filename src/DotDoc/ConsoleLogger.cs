using DotDoc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc
{
    public class ConsoleLogger : BaseLogger
    {
        public ConsoleLogger() : this(LogLevel.Info)
        {
        }
        
        public ConsoleLogger(LogLevel level) : base(level)
        {
        }

        protected override void WriteTrace(string message)
        {
            Write(ConsoleColor.Cyan, "Trace", message);
        }

        protected override void WriteInfo(string message)
        {
            Write(ConsoleColor.Blue, "Info", message);
        }
        
        protected override void WriteWarn(string message)
        {
            Write(ConsoleColor.Magenta, "Warn", message);
        }

        protected override void WriteError(string message)
        {
            Write(ConsoleColor.Red, "Error", message);
        }

        private void Write(ConsoleColor color, string header, string message)
        {
            Console.ForegroundColor = color;
            Console.Write($"{header}: ");
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }
}
