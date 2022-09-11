# BasePage Class

namespace: [DotDoc\.Core\.Write\.Page](../DotDoc.Core.Write.Page.md)<br />
assembly: [DotDoc](../../DotDoc.md)



```csharp
public abstract class BasePage;
```

Inheritance: [object](https://docs.microsoft.com/ja-jp/dotnet/api/System.Object) > BasePage

## Constructors

| Name | Summary |
|------|---------|
| [BasePage\(DotDoc\.Core\.Models\.IDocItem, DotDoc\.Core\.Write\.TextTransform, DotDoc\.Core\.Write\.DocItemContainer\)](./BasePage/$ctor.md) |  |

## Methods

| Name | Summary |
|------|---------|
| [AppendTitle\(System\.Text\.StringBuilder, string, int\)](./BasePage/AppendTitle.md) |  |
| [AppendExample\(System\.Text\.StringBuilder, DotDoc\.Core\.Models\.IDocItem, DotDoc\.Core\.Models\.IDocItem, int\)](./BasePage/AppendExample.md) |  |
| [AppendRemarks\(System\.Text\.StringBuilder, DotDoc\.Core\.Models\.IDocItem, DotDoc\.Core\.Models\.IDocItem, int\)](./BasePage/AppendRemarks.md) |  |
| [AppendItemList\<T\>\(System\.Text\.StringBuilder, string, System\.Collections\.Generic\.IEnumerable\<DotDoc\.Core\.Models\.IDocItem\>, int\)](./BasePage/AppendItemList.md) |  |
| [AppendFieldItemList\(System\.Text\.StringBuilder, System\.Collections\.Generic\.IEnumerable\<DotDoc\.Core\.Models\.IDocItem\>, int, bool\)](./BasePage/AppendFieldItemList.md) |  |
| [AppendTypeParameterList\(System\.Text\.StringBuilder, System\.Collections\.Generic\.IEnumerable\<DotDoc\.Core\.Models\.TypeParameterDocItem\>, int\)](./BasePage/AppendTypeParameterList.md) |  |
| [AppendDeclareCode\(System\.Text\.StringBuilder\)](./BasePage/AppendDeclareCode.md) |  |
| [AppendNamespaceAssemblyInformation\(System\.Text\.StringBuilder, string, string, int\)](./BasePage/AppendNamespaceAssemblyInformation.md) |  |
| [AppendAssemblyInformation\(System\.Text\.StringBuilder, string, int\)](./BasePage/AppendAssemblyInformation.md) |  |
| [AppendInheritAndImplements\(System\.Text\.StringBuilder, DotDoc\.Core\.Models\.TypeDocItem\)](./BasePage/AppendInheritAndImplements.md) |  |
| [AppendParameterList\(System\.Text\.StringBuilder, System\.Collections\.Generic\.IEnumerable\<DotDoc\.Core\.Models\.ParameterDocItem\>, int\)](./BasePage/AppendParameterList.md) |  |
| [AppendReturnValue\(System\.Text\.StringBuilder, DotDoc\.Core\.Models\.ReturnItem, int\)](./BasePage/AppendReturnValue.md) |  |

