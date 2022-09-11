# BaseLogger Class

namespace: [DotDoc\.Core](../DotDoc.Core.md)<br />
assembly: [DotDoc](../../DotDoc.md)

ログを出力する基本機能を実装します。

```csharp
public abstract class BaseLogger;
```

Inheritance: [object](https://docs.microsoft.com/dotnet/api/System.Object) > BaseLogger

Implements: [DotDoc\.Core\.ILogger](../../DotDoc/DotDoc.Core/ILogger.md)

## Constructors

| Name | Summary |
|------|---------|
| [BaseLogger\(DotDoc\.Core\.LogLevel\)](./BaseLogger/$ctor.md) | 出力するログレベルを指定してインスタンスを生成します。 |

## Methods

| Name | Summary |
|------|---------|
| [Trace\(string\)](./BaseLogger/Trace.md) | トレースレベルのログを出力します。 |
| [WriteTrace\(string\)](./BaseLogger/WriteTrace.md) | トレースレベルのログの出力を実装します。 |
| [Info\(string\)](./BaseLogger/Info.md) | インフォメーションレベルのログを出力します。 |
| [WriteInfo\(string\)](./BaseLogger/WriteInfo.md) | インフォメーションレベルのログの出力を実装します。 |
| [Warn\(string\)](./BaseLogger/Warn.md) | 警告レベルのログを出力します。 |
| [WriteWarn\(string\)](./BaseLogger/WriteWarn.md) | 警告レベルのログの出力を実装します。 |
| [Error\(string\)](./BaseLogger/Error.md) | エラーレベルのログを出力します。 |
| [WriteError\(string\)](./BaseLogger/WriteError.md) | エラーレベルのログの出力を実装します。 |

