using DotDoc.Core.Read;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class FieldDocItem : MemberDocItem
{
    public FieldDocItem(IFieldSymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
        ConstantValue = symbol.ConstantValue;
        IsStatic = symbol.IsStatic;
        IsReadOnly = symbol.IsReadOnly;
        IsConstant = symbol.IsConst;
        IsVolatile = symbol.IsVolatile;
        TypeInfo = symbol.Type.ToTypeInfo();
    }
        
    public TypeInfo TypeInfo { get; }
        
    public bool IsStatic { get; }
        
    public bool IsReadOnly { get; }
        
    public object? ConstantValue { get; }
        
    public bool IsConstant { get; }
        
    public bool IsVolatile { get; }

    public override string ToDeclareCSharpCode()
    {
        var modifiers = new List<string>();
        if(IsStatic && !IsConstant) modifiers.Add("static ");
        if(IsConstant) modifiers.Add("const ");
        if(IsVolatile) modifiers.Add("volatile ");
        if(IsReadOnly) modifiers.Add("readonly ");
        return $"{Accessiblity.ToCSharpText()} {string.Join("", modifiers)}{TypeInfo.DisplayName} {DisplayName}{(!IsConstant ? "" : " = " + GetConstantValueDisplayText(ConstantValue))};";
    }
            
    private string GetConstantValueDisplayText(object? value)
    {
        if (value is null) return "null";
        return value is char ? $"'{value}'" :
            value is string ? $"\"{value}\"" : value.ToString();
    }
}