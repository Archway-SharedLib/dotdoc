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
    public int ReturnIntWithParam(string param1, int param2) => param2;

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
    
}