namespace SampleLib;

/// <summary>
/// プロパティを検証するためのabstractクラスです。
/// </summary>
public abstract class BasePropertyVariation
{
    /// <summary>
    /// protected なベースプロパティです、
    /// </summary>
    protected string BaseProtected { get; }
    
    /// <summary>
    /// virtual なベースプロパティです。
    /// </summary>
    protected virtual string BaseVirtual { get; }
    
    /// <summary>
    /// abstract なベースプロパティです。
    /// </summary>
    protected abstract string BaseAbstract { get; init; }
}

public class PropertyVariation : BasePropertyVariation
{
    public string ReadWrite { get; set; }
    
    public string Read { get; }
    
    public string Write {
        set { }
    }

    public string InitWrite
    {
        init { }
    }
    
    public string ReadInit { get; init; }

    public static string StaticProp { get; set; }
    
    protected new string BaseProtected { get; }

    /// <inheritdoc />
    protected override string BaseVirtual { get; }
    
    /// <inheritdoc />
    protected override string BaseAbstract { get; init; }
}