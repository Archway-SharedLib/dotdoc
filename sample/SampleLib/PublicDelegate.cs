using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLib
{
    /// <summary>
    /// パブリックなデリゲートです。
    /// </summary>
    /// <param name="value">引数です。</param>
    /// <returns>戻り値です</returns>
    public delegate string PublicDelegate(string value);

    /// <summary>
    /// パブリックなTをもつデリゲートです。
    /// </summary>
    /// <param name="value">引数です。</param>
    /// <returns>戻り値です</returns>
    /// <typeparam name="T">Tです</typeparam>
    public delegate T PublicDelegate<T>(string value);
}
