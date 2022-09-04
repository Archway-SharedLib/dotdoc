using System;
using System.Collections.Generic;

namespace Test;

public interface BaseInterface
{
    /// <summary>
    /// <see cref="BaseInterface"/> の <see cref="IM"/> のサマリです。
    /// </summary>
    /// <param name="a">BaseInterface a 引数です</param>
    /// <returns><see cref="int"/> </returns>
    /// <returns><see cref="int"/> です。</returns>
    int IM(DateTime a);
}

public class BaseType
{
    /// <summary>
    /// <see cref="BaseType"/> の <see cref="M"/> のサマリです。
    /// </summary>
    /// <param name="a">BaseType a 引数です</param>
    /// <param name="b">Baseype b 引数です</param>
    /// <returns><see cref="BaseType"/> の <see cref="M"/> の戻り値です。</returns>
    public virtual string M(string a, int b) => a;
}

public class InheritDoc: BaseType
{
    public override string M(string a, int b) => a;
}

public class NoInheritDoc
{
    /// <inheritdoc cref="BaseType.M"/>
    public string M(string a, int b) => a;
}

/// <summary>
/// <c>null</c>
/// </summary>
public class BaseImplDoc: BaseInterface
{
    /// <inheritdoc />
    public int IM(DateTime a) => a.Millisecond;
}