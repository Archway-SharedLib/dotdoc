using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLib.SubNs
{
    /// <summary>
    /// Abstractクラスです。
    /// </summary>
    public abstract class AbstractClass
    {
        /// <summary>
        /// ReadOnlyのAbstractプロパティです。
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Abstractなメソッドです。
        /// </summary>
        /// <param name="date">日付です。</param>
        /// <returns>文字列です。</returns>
        public abstract string AbstractMethod(DateTimeOffset date);

    }
}
