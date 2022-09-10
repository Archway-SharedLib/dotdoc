using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class EventDocItem : MemberDocItem
{
    public EventDocItem(IEventSymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
    }
        
    public override string ToDeclareCSharpCode() => string.Empty;
}