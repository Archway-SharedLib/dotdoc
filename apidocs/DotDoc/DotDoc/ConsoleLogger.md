# ConsoleLogger Class

namespace: [DotDoc](../DotDoc.md)<br />
assembly: [DotDoc](../../DotDoc.md)

コンソールにログを出力します。

```csharp
public class ConsoleLogger;
```

Inheritance: [object](https://docs.microsoft.com/dotnet/api/System.Object) > [DotDoc\.Core\.BaseLogger](../../DotDoc/DotDoc.Core/BaseLogger.md) > ConsoleLogger

## Constructors

| Name | Summary |
|------|---------|
| [ConsoleLogger\(\)](./ConsoleLogger/$ctor.md) | デフォルトのログレベルを利用してインスタンスを初期化します。 |
| [ConsoleLogger\(DotDoc\.Core\.LogLevel\)](./ConsoleLogger/$ctor.md) | 出力するログレベルを指定してインスタンスを生成します。 |

## Methods

| Name | Summary |
|------|---------|
| [WriteTrace\(string\)](./ConsoleLogger/WriteTrace.md) | トレースレベルのログの出力を実装します。 |
| [WriteInfo\(string\)](./ConsoleLogger/WriteInfo.md) | インフォメーションレベルのログの出力を実装します。 |
| [WriteWarn\(string\)](./ConsoleLogger/WriteWarn.md) | 警告レベルのログの出力を実装します。 |
| [WriteError\(string\)](./ConsoleLogger/WriteError.md) | エラーレベルのログの出力を実装します。 |

