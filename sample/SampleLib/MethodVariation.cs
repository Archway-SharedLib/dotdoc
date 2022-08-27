namespace SampleLib;

/// <summary>
/// プロパティを検証するためのabstractクラスです。
/// </summary>
public abstract class BaseMethodVariation
{
    /// <summary>
    /// Baseメソッドです
    /// </summary>
    public void BaseMethod()
    {
    }
    
    /// <summary>
    /// バーチャルです。
    /// </summary>
    public virtual void VirtualMethod(){}

    /// <summary>
    /// abstractです。
    /// </summary>
    public abstract void AbstractMethod();
}

public class MethodVariation : BaseMethodVariation
{
    /// <summary>
    /// リターンなし引数なし
    /// </summary>
    public void ReturnVoidNoParam()
    {
    }

    /// <summary>
    /// リターン文字列引数なし
    /// </summary>
    /// <returns>文字列の <see cref="string"/> を返します。</returns>
    public string ReturnStringNoParam() => string.Empty;

    /// <summary>
    /// リターンなし引数2
    /// </summary>
    /// <param name="param1"> 文字列の <see cref="string"/> です </param>
    /// <param name="param2"> 数字の <see cref="int"/> です</param>
    public void ReturnVoidWithParam(string param1, int param2)
    {
    }

    /// <summary>
    /// リターン数字引数2
    /// </summary>
    /// <param name="param1"> 文字列の <see cref="string"/> です </param>
    /// <param name="param2"> 数字の <see cref="int"/> です</param>
    /// <returns>数字の <see cref="int"/> を返します。</returns>
    public int ReturnIntWithParam(in string param1, ref int param2, out int param3)
    {
        param3 = 1;
        return 1;
    }

    public readonly struct S1
    {
    }
    
    public ref readonly int ReturnRefReadOnly(in int v)
    {
        ref readonly var result = ref v;
        return ref result;
    }

    /// <inheritdoc />
    public override void AbstractMethod()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override void VirtualMethod()
    {
        base.VirtualMethod();
    }
    
    public new void BaseMethod(){}
    
    
    /// <summary>
    /// リターンなし引数なし
    /// </summary>
    public void OverloadMethod()
    {
    }

    /// <summary>
    /// 文字引数
    /// </summary>
    /// <param name="str">Strです。</param>
    public void OverloadMethod(string str)
    {
    }

    /// <summary>
    /// 数字引数
    /// </summary>
    /// <param name="num">Numです。</param>
    public void OverloadMethod(int num)
    {
    }

    /// <summary>
    /// 文字数字引数
    /// </summary>
    /// <param name="num">Numです。</param>
    /// <param name="str">Strです。</param>
    /// <returns>strです</returns>
    public string OverloadMethod(int num, string str) => str;
    
    /// <summary>
    /// 文字T引数
    /// </summary>
    /// <param name="num">Numです。</param>
    /// <param name="str">Strです。</param>
    /// <typeparam name="T">Tです</typeparam>
    /// <returns>strです</returns>
    public string OverloadMethod<T>(T num, string str) => str;
    
    /// <summary>
    /// T1T2引数
    /// </summary>
    /// <param name="num">Numです。</param>
    /// <param name="str">Strです。</param>
    /// <typeparam name="T">Tです</typeparam>
    /// <typeparam name="T2">T2です</typeparam>
    /// <returns>T2です</returns>
    public T2 OverloadMethod<T, T2>(T num, T2 str) => str;
}