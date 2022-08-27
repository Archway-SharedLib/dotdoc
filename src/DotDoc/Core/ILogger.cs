using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core
{
    public interface ILogger
    {
        void Trace(string message);
        
        void Info(string message);

        void Warn(string message);
        
        void Error(string message);
    }

    public enum LogLevel
    {
        None = 0,
        Trace = 1,
        Info = 2,
        Warn = 3,
        Error = 4
    }

    public abstract class BaseLogger : ILogger
    {
        private readonly LogLevel _level;

        protected BaseLogger(LogLevel level)
        {
            _level = level;
        }

        private void Write(LogLevel targetLevel, string message, Action<string> writer)
        {
            if (_level >= targetLevel)
            {
                writer(message);
            }
        }

        public void Trace(string message) => Write(LogLevel.Trace, message, WriteTrace);

        protected abstract void WriteTrace(string message);
        
        public void Info(string message) => Write(LogLevel.Info, message, WriteInfo);

        protected abstract void WriteInfo(string message);

        public void Warn(string message) => Write(LogLevel.Warn, message, WriteWarn);

        protected abstract void WriteWarn(string message);

        public void Error(string message) => Write(LogLevel.Error, message, WriteError);

        protected abstract void WriteError(string message);
    }
}
