# ConsoleLogger Class

namespace: [DotDoc](../DotDoc.md)<br />
assembly: [DotDoc](../../DotDoc.md)

コンソールにログを出力します。

```csharp
public class ConsoleLogger;
```

Inheritance: [object](https://docs.microsoft.com/dotnet/api/System.Object) > [BaseLogger](../../DotDoc/DotDoc.Core/BaseLogger.md) > ConsoleLogger

## Example

ログを出力するコード例を記載します。

``` cs
var logger = new ConsoleLogger(LogLevel.Error);
logger.Info("インフォメーションメッセージ");
```



## Remarks

通常、このクラスは直接インスタンス化せず、 [ILogger](../../DotDoc/DotDoc.Core/ILogger.md) 型を経由して利用します。


このクラスから出力されたメッセージの先頭にはログレベルが付与されます。

``` cs
logger.Info("インフォメーションメッセージです。");
```


``` 
Info: インフォメーションメッセージです。
```




## Constructors

| Name | Summary |
|------|---------|
| [ConsoleLogger\(\)](./ConsoleLogger/$ctor.md) | デフォルトのログレベルを利用してインスタンスを初期化します。 |
| [ConsoleLogger\(LogLevel\)](./ConsoleLogger/$ctor.md) | 出力するログレベルを指定してインスタンスを生成します。 |

## Methods

| Name | Summary |
|------|---------|
| [WriteTrace\(string\)](./ConsoleLogger/WriteTrace.md) | トレースレベルのログの出力を実装します。 |
| [WriteInfo\(string\)](./ConsoleLogger/WriteInfo.md) | インフォメーションレベルのログの出力を実装します。 |
| [WriteWarn\(string\)](./ConsoleLogger/WriteWarn.md) | 警告レベルのログの出力を実装します。 |
| [WriteError\(string\)](./ConsoleLogger/WriteError.md) | エラーレベルのログの出力を実装します。 |

