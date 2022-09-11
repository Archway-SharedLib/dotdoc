using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core
{
    /// <summary>
    /// ログ出力するインターフェイスを定義します。
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// トレースレベルのログを出力します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        void Trace(string message);
        
        /// <summary>
        /// インフォメーションレベルのログを出力します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        void Info(string message);

        /// <summary>
        /// 警告レベルのログを出力します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        void Warn(string message);
        
        /// <summary>
        /// エラーレベルのログを出力します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        void Error(string message);
    }

    /// <summary>
    /// ログのレベルを定義します。
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// すべてのレベル
        /// </summary>
        All = 0,
        /// <summary>
        /// トレースレベル
        /// </summary>
        Trace = 1,
        /// <summary>
        /// インフォメーションレベル
        /// </summary>
        Info = 2,
        /// <summary>
        /// 警告レベル
        /// </summary>
        Warn = 3,
        /// <summary>
        /// エラーレベル
        /// </summary>
        Error = 4,
        /// <summary>
        /// レベル指定なし
        /// </summary>
        None = 99
    }

    /// <summary>
    /// ログを出力する基本機能を実装します。
    /// </summary>
    public abstract class BaseLogger : ILogger
    {
        private readonly LogLevel _level;

        /// <summary>
        /// 出力するログレベルを指定してインスタンスを生成します。
        /// </summary>
        /// <param name="level">出力するログレベル</param>
        protected BaseLogger(LogLevel level)
        {
            _level = level;
        }

        private void Write(LogLevel targetLevel, string message, Action<string> writer)
        {
            if (_level <= targetLevel)
            {
                writer(message);
            }
        }

        /// <inheritdoc />
        public void Trace(string message) => Write(LogLevel.Trace, message, WriteTrace);

        /// <summary>
        /// トレースレベルのログの出力を実装します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        protected abstract void WriteTrace(string message);
        
        /// <inheritdoc />
        public void Info(string message) => Write(LogLevel.Info, message, WriteInfo);

        /// <summary>
        /// インフォメーションレベルのログの出力を実装します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        protected abstract void WriteInfo(string message);

        /// <inheritdoc />
        public void Warn(string message) => Write(LogLevel.Warn, message, WriteWarn);

        /// <summary>
        /// 警告レベルのログの出力を実装します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        protected abstract void WriteWarn(string message);

        /// <inheritdoc />
        public void Error(string message) => Write(LogLevel.Error, message, WriteError);

        /// <summary>
        /// エラーレベルのログの出力を実装します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        protected abstract void WriteError(string message);
    }
}
