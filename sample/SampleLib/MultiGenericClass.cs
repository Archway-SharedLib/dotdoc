using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleLib
{
    /// <summary>
    /// 複数の型パラメーターのジェネリッククラス
    /// </summary>
    /// <typeparam name="T1">1個めの型パラメーター。<see cref="string"/>です。</typeparam>
    /// <typeparam name="T2">2個めの型パラメーター</typeparam>
    /// <typeparam name="T3">3個めの型パラメーター</typeparam>
    /// <typeparam name="T4">4個めの型パラメーター</typeparam>
    /// <typeparam name="T5">5個めの型パラメーター</typeparam>
    /// <typeparam name="T6">6個めの型パラメーター</typeparam>
    /// <typeparam name="T7">7個めの型パラメーター。<see cref="PublicClass"/>です。</typeparam>
    /// <typeparam name="T8">8個めの型パラメーター</typeparam>
    /// <typeparam name="T9">9個めの型パラメーター</typeparam>
    /// <typeparam name="T10">10個めの型パラメーター</typeparam>
    /// <typeparam name="T11">11個めの型パラメーター</typeparam>
    /// <typeparam name="T12">12個めの型パラメーター</typeparam>
    public class MultiGenericClass<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
        where T1: struct
        where T2: class
        where T3: class?
        where T4: notnull
        where T5: unmanaged
        where T6: new()
        where T7: PublicClass
        where T8: PublicClass?
        where T9: IPublicInterface
        where T10: IPublicInterface?
        where T11: T12
    {
    }
}
