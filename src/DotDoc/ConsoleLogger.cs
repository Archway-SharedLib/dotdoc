using DotDoc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc
{
    /// <summary>
    /// コンソールにログを出力します。
    /// </summary>
    /// <remarks>
    /// 通常、このクラスは直接インスタンス化せず、 <see cref="ILogger" /> 型を経由して利用します。
    /// <para>
    /// このクラスから出力されたメッセージの先頭にはログレベルが付与されます。
    /// <code language="cs">
    /// <![CDATA[
    /// logger.Info("インフォメーションメッセージです。");
    /// ]]>
    /// </code>
    /// <code language="">
    /// <![CDATA[
    /// Info: インフォメーションメッセージです。
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    /// <example>
    /// ログを出力するコード例を記載します。
    /// <code language="cs">
    /// <![CDATA[
    /// var logger = new ConsoleLogger(LogLevel.Error);
    /// logger.Info("インフォメーションメッセージ");
    /// ]]>
    /// </code>
    /// </example>
    public class ConsoleLogger : BaseLogger
    {
        /// <summary>
        /// デフォルトのログレベルを利用してインスタンスを初期化します。
        /// </summary>
        /// <remarks>
        /// このコンストラクターを使用した場合、ログレベルはインフォメーションレベルが指定されます。
        /// </remarks>
        public ConsoleLogger() : this(LogLevel.Info)
        {
        }
        
        /// <summary>
        /// 出力するログレベルを指定してインスタンスを生成します。
        /// </summary>
        /// <param name="level">出力するログレベル</param>
        public ConsoleLogger(LogLevel level) : base(level)
        {
        }

        /// <inheritdoc />
        protected override void WriteTrace(string message)
        {
            Write(ConsoleColor.Cyan, "Trace", message);
        }

        /// <inheritdoc />
        protected override void WriteInfo(string message)
        {
            Write(ConsoleColor.Blue, "Info", message);
        }
        
        /// <inheritdoc />
        protected override void WriteWarn(string message)
        {
            Write(ConsoleColor.Magenta, "Warn", message);
        }

        /// <inheritdoc />
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
