namespace SampleLib;

/// <summary>
/// <see cref="ISource"/> の Summary です。
/// </summary>
public interface ISource
{
}

/// <inheritdoc />
public class Source : ISource
{
}

/// <inheritdoc />
public class BaseSource : ISource
{
}

/// <inheritdoc />
public class InheritBaseSource : BaseSource
{
}

/// <summary>
/// <see cref="OverwriteSource"/> のコメントです。
/// </summary>
public class OverwriteSource : ISource
{
    /// <summary>
    /// サマリー
    /// </summary>
    /// <param name="p1">param1</param>
    /// <param name="p2">param2</param>
    /// <returns>戻り</returns>
    public virtual string M(string p1, int p2) => string.Empty;
}

/// <inheritdoc />
/// <summary></summary>
public class InheritanceOverwriteSource : OverwriteSource
{
    /// <inheritdoc />
    public override string M(string p1, int p2)
    {
        return base.M(p1, p2);
    }
}

/// <summary>
/// <see cref="ISecondSource"/> の Summary です。
/// </summary>
public interface ISecondSource
{
}

/// <inheritdoc />
public class MixSource:  ISource, ISecondSource{}

/// <inheritdoc cref="ISecondSource" />
public class MixSource2:  ISource, ISecondSource{}
