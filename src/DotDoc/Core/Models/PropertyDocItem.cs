using DotDoc.Core.Read;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class PropertyDocItem : MemberDocItem
{
    public PropertyDocItem(IPropertySymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
        TypeInfo = symbol.Type.ToTypeInfo();
        HasGet = symbol.GetMethod is not null;
        HasSet = symbol.SetMethod is not null;
        IsInit = symbol.SetMethod?.IsInitOnly ?? false;
        IsStatic = symbol.IsStatic;
        IsAbstract = symbol.IsAbstract;
        IsOverride = symbol.IsOverride;
        IsVirtual = symbol.IsVirtual;
    }
        
    public TypeInfo TypeInfo { get; }
        
    public bool HasGet { get; }
        
    public bool HasSet { get; }
        
    public bool IsInit { get; }

    public bool IsStatic { get; }
        
    public bool IsOverride { get; }
        
    public bool IsVirtual { get; }
        
    public bool IsAbstract { get; }
        
    public override string ToDeclareCSharpCode()
    {
        var getset = HasGet && HasSet ? "get; " + (IsInit ? "init;" : "set;") :
            HasGet && !HasSet ? "get;" :
            !HasGet && HasSet ? (IsInit ? "init;" : "set;") : string.Empty;
        var modifiers = new List<string>();
        if(IsAbstract) modifiers.Add("abstract ");
        if(IsStatic) modifiers.Add("static ");
        if(IsOverride) modifiers.Add("override ");
        if(IsVirtual) modifiers.Add("virtual ");
            
        return $"{Accessiblity.ToCSharpText()} {string.Join("", modifiers)}{TypeInfo.DisplayName} {DisplayName} {{ {getset} }};";
    }
}