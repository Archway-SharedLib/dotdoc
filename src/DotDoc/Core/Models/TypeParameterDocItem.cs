using DotDoc.Core.Read;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class TypeParameterDocItem : DocItem
{
    public TypeParameterDocItem(ITypeParameterSymbol symbol, XmlDocInfo docInfo, Compilation compilation) : base(symbol, compilation)
    {
        XmlDocText = docInfo?.TypeParameters.OrEmpty().FirstOrDefault(p => p.Name == symbol.Name)?.Text;
        HasConstructorConstraint = symbol.HasConstructorConstraint;
        HasReferenceTypeConstraint = symbol.HasReferenceTypeConstraint;
        HasValueTypeConstraint = symbol.HasValueTypeConstraint;
        HasNotNullConstraint = symbol.HasNotNullConstraint;
        HasUnmanagedTypeConstraint = symbol.HasUnmanagedTypeConstraint;
        HasReferenceTypeConstraintNullableAnnotation =
            symbol.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated;

        ConstraintTypes = symbol.ConstraintTypes.Select(t => t.ToTypeInfo()).ToList();
    }
    
    public bool HasConstructorConstraint { get; }
    
    public bool HasReferenceTypeConstraint { get; }
    
    public bool HasValueTypeConstraint { get; }
    
    public bool HasNotNullConstraint { get; }
    
    public bool HasUnmanagedTypeConstraint { get; }
    
    public bool HasReferenceTypeConstraintNullableAnnotation { get; }
    
    public IList<TypeInfo> ConstraintTypes { get; }

    public string? XmlDocText { get; }

    public override string ToDeclareCSharpCode()
    {
        var hasConstraint = HasConstructorConstraint || HasReferenceTypeConstraint || HasValueTypeConstraint ||
                            HasNotNullConstraint || HasUnmanagedTypeConstraint;
        if (!hasConstraint) return string.Empty;
        var constraints = new List<string>();

        foreach (var type in ConstraintTypes)
        {
            constraints.Add(type.DisplayName);
        }
        
        if(HasConstructorConstraint) constraints.Add("new()");
        if(HasReferenceTypeConstraint) constraints.Add($"class{(HasReferenceTypeConstraintNullableAnnotation ? "?" : "")}");
        if(HasValueTypeConstraint) constraints.Add($"struct");
        if(HasNotNullConstraint) constraints.Add($"notnull");
        if(HasUnmanagedTypeConstraint) constraints.Add($"unmanaged");

        return $"where {Name} : {(string.Join(", ", constraints))}";
    }
}