# AppendJoin Method

namespace: [DotDoc\.Core\.Write](../../DotDoc.Core.Write.md)<br />
assembly: [DotDoc](../../../DotDoc.md)



## Overloads

| Name | Summary |
|------|---------|
| AppendJoin\(char, params string?\[\]\) | 指定された配列を指定された区切り文字で連結した文字列を挿入します。 |
| AppendJoin\(string?, params string?\[\]\) | 指定された配列を指定された区切り文字で連結した文字列を挿入します。 |
| AppendJoin\<T\>\(string?, IEnumerable\<T\>\) | 指定された列挙可能な値を指定された区切り文字で連結した文字列を挿入します。 |

## AppendJoin\(char, params string?\[\]\)

指定された配列を指定された区切り文字で連結した文字列を挿入します。

```csharp
public TextBuilder AppendJoin(char separator ,params string?[] values);
```

### Parameters

__separator__ : [char](https://docs.microsoft.com/dotnet/api/System.Char)

区切り文字

__values__ : [string?](https://docs.microsoft.com/dotnet/api/System.String)

値

### Return Value

[TextBuilder](../../../DotDoc/DotDoc.Core.Write/TextBuilder.md)

自身のインスタンス

## AppendJoin\(string?, params string?\[\]\)

指定された配列を指定された区切り文字で連結した文字列を挿入します。

```csharp
public TextBuilder AppendJoin(string? separator ,params string?[] values);
```

### Parameters

__separator__ : [string?](https://docs.microsoft.com/dotnet/api/System.String)

区切り文字

__values__ : [string?](https://docs.microsoft.com/dotnet/api/System.String)

値

### Return Value

[TextBuilder](../../../DotDoc/DotDoc.Core.Write/TextBuilder.md)

自身のインスタンス

## AppendJoin\<T\>\(string?, IEnumerable\<T\>\)

指定された列挙可能な値を指定された区切り文字で連結した文字列を挿入します。

```csharp
public TextBuilder AppendJoin<T>(string? separator ,IEnumerable<T> values);
```

### Type Parameters

__T__



### Parameters

__separator__ : [string?](https://docs.microsoft.com/dotnet/api/System.String)

区切り文字

__values__ : [IEnumerable\<T\>](https://docs.microsoft.com/dotnet/api/System.Collections.Generic.IEnumerable-1)

値

### Return Value

[TextBuilder](../../../DotDoc/DotDoc.Core.Write/TextBuilder.md)

自身のインスタンス

