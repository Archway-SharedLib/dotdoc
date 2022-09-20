using System.Text;

namespace DotDoc.Core.Write
{
    /// <summary>
    /// <see cref="StringBuilder"/> のラッパーを定義します。
    /// </summary>
    public class TextBuilder
    {
        private const string DefaultNewLine = "\n";

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public TextBuilder(): this(new StringBuilder())
        {
        }

        /// <summary>
        /// 元となる <see cref="StringBuilder"/> を指定してインスタンスを初期化します。
        /// </summary>
        /// <param name="source">元となる <see cref="StringBuilder"/></param>
        public TextBuilder(StringBuilder source)
        {
            Source = source;
            NewLine = DefaultNewLine;
        }

        /// <summary>
        /// 元となる <see cref="StringBuilder"/> を取得します。
        /// </summary>
        public StringBuilder Source { get; }

        /// <summary>
        /// 改行コードを取得します。
        /// </summary>
        public string NewLine { get; }

        /// <summary>
        /// 指定された文字列を追加します。
        /// </summary>
        /// <param name="value">追加する文字列</param>
        /// <returns>自身のインスタンス</returns>
        public TextBuilder Append(string value)
        {
            Source.Append(value);
            return this;
        }

        /// <summary>
        /// 指定された文字列を追加して、末尾に改行コードを挿入します。
        /// </summary>
        /// <param name="value">追加する文字列</param>
        /// <returns>自身のインスタンス</returns>
        public TextBuilder AppendLine(string value)
        {
            Source.Append(value).Append(NewLine);
            return this;
        }

        /// <summary>
        /// 改行コードを挿入します。
        /// </summary>
        /// <returns>自身のインスタンス</returns>
        public TextBuilder AppendLine()
        {
            Source.Append(NewLine);
            return this;
        }

        /// <summary>
        /// 指定された配列を指定された区切り文字で連結した文字列を挿入します。
        /// </summary>
        /// <param name="separator">区切り文字</param>
        /// <param name="values">値</param>
        /// <returns>自身のインスタンス</returns>
        public TextBuilder AppendJoin(char separator, params string?[] values)
        {
            Source.AppendJoin(separator, values);
            return this;
        }

        /// <summary>
        /// 指定された配列を指定された区切り文字で連結した文字列を挿入します。
        /// </summary>
        /// <param name="separator">区切り文字</param>
        /// <param name="values">値</param>
        /// <returns>自身のインスタンス</returns>
        public TextBuilder AppendJoin(string? separator, params string?[] values)
        {
            Source.AppendJoin(separator, values);
            return this;
        }

        /// <summary>
        /// 指定された列挙可能な値を指定された区切り文字で連結した文字列を挿入します。
        /// </summary>
        /// <param name="separator">区切り文字</param>
        /// <param name="values">値</param>
        /// <returns>自身のインスタンス</returns>
        public TextBuilder AppendJoin<T>(string? separator, IEnumerable<T> values)
        {
            Source.AppendJoin(separator, values);
            return this;
        }
    
        /// <summary>
        /// インスタンを文字列に変換します。
        /// </summary>
        /// <returns>文字列</returns>
        public override string ToString()
            => Source.ToString();
    }
}
