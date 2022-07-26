namespace SampleLib.Privates;

/// <summary>
/// プライベートなタイプを定義します。
/// </summary>
public class PrivateDefinitions
{
    /// <summary>
    /// プライベートクラスです。
    /// </summary>
    private class PrivateClass
    {
    }
    /// <summary>
    /// プライベートインターフェイスです。
    /// </summary>
    private interface PrivateInterface
    {
    }
    /// <summary>
    /// プライベート列挙体です。
    /// </summary>
    private enum PrivateEnum
    {
    }
    /// <summary>
    /// プライベートデリゲートです。
    /// </summary>
    /// <param name="para"><see cref="bool"/>です</param>
    /// <returns><see cref="int"/>です。</returns>
    private delegate int PrivateDelegate(bool para);
    /// <summary>
    /// プライベート構造体です。
    /// </summary>
    private struct PrivateStruct
    {
    }
}