# TextBuilder Class

namespace: [DotDoc\.Core\.Write](../DotDoc.Core.Write.md)<br />
assembly: [DotDoc](../../DotDoc.md)

[System\.Text\.StringBuilder](https://docs.microsoft.com/dotnet/api/System.Text.StringBuilder) のラッパーを定義します。

```csharp
public class TextBuilder;
```

Inheritance: [object](https://docs.microsoft.com/dotnet/api/System.Object) > TextBuilder

## Constructors

| Name | Summary |
|------|---------|
| [TextBuilder\(\)](./TextBuilder/$ctor.md) | インスタンスを初期化します。 |
| [TextBuilder\(StringBuilder\)](./TextBuilder/$ctor.md) | 元となる [System\.Text\.StringBuilder](https://docs.microsoft.com/dotnet/api/System.Text.StringBuilder) を指定してインスタンスを初期化します。 |

## Methods

| Name | Summary |
|------|---------|
| [Append\(string\)](./TextBuilder/Append.md) | 指定された文字列を追加します。 |
| [AppendLine\(string\)](./TextBuilder/AppendLine.md) | 指定された文字列を追加して、末尾に改行コードを挿入します。 |
| [AppendLine\(\)](./TextBuilder/AppendLine.md) | 改行コードを挿入します。 |
| [AppendJoin\(char, params string?\[\]\)](./TextBuilder/AppendJoin.md) | 指定された配列を指定された区切り文字で連結した文字列を挿入します。 |
| [AppendJoin\(string?, params string?\[\]\)](./TextBuilder/AppendJoin.md) | 指定された配列を指定された区切り文字で連結した文字列を挿入します。 |
| [AppendJoin\<T\>\(string?, IEnumerable\<T\>\)](./TextBuilder/AppendJoin.md) | 指定された列挙可能な値を指定された区切り文字で連結した文字列を挿入します。 |
| [ToString\(\)](./TextBuilder/ToString.md) | インスタンを文字列に変換します。 |

## Properties

| Name | Summary |
|------|---------|
| [Source](./TextBuilder/Source.md) | 元となる [System\.Text\.StringBuilder](https://docs.microsoft.com/dotnet/api/System.Text.StringBuilder) を取得します。 |
| [NewLine](./TextBuilder/NewLine.md) | 改行コードを取得します。 |

