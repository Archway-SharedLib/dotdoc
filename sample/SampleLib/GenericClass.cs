using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLib
{
    /// <summary>
    /// 型パラメーター1このジェネリッククラス
    /// </summary>
    /// <typeparam name="T">型パラメーター1</typeparam>
    public class GenericClass<T>
    {

        /// <summary>
        /// ジェネリックメソッドです。
        /// </summary>
        /// <typeparam name="TArg">メソッドの型パラメーターです</typeparam>
        /// <param name="arg">引数です。</param>
        /// <returns>戻り値です。</returns>
        public T GenericMethod<TArg>(TArg arg) where TArg : T
        {
            return arg;
        }

    }
}
