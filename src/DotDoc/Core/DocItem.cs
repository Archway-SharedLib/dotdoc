using DotDoc.Core.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core
{
    public interface IDocItem
    {
        string? Id { get; }
        
        string? DisplayName { get; }
        
        IEnumerable<IDocItem>? Items { get; }
        
        XmlDocInfo? XmlDocInfo { get; }
    }
    
    public interface IHaveParameters
    {
        public List<ParameterDocItem>? Parameters { get; set; }
    }

    public interface IHaveTypeParameters
    {
        public List<TypeParameterDocItem>? TypeParameters { get; set; }
    }
    
    public interface IHaveReturnValue
    {
        public ReturnDocItem? ReturnValue { get; set; }
    }

    public abstract class DocItem : IDocItem
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }
        
        public string? MetadataName { get; set; }

        public XmlDocInfo? XmlDocInfo { get; set; }

        public virtual IEnumerable<IDocItem>? Items { get; } = Enumerable.Empty<IDocItem>();

        public Accessibility Accessiblity { get; set; } = Accessibility.Unknown;

        public abstract string ToDeclareCSharpCode();

    }

    public class AssemblyDocItem : DocItem
    {
        public List<NamespaceDocItem>? Namespaces { get; set; }

        public override IEnumerable<IDocItem>? Items => Namespaces;

        public override string ToDeclareCSharpCode() => string.Empty;
    }

    public class NamespaceDocItem : DocItem
    {
        public List<TypeDocItem>? Types { get; set; }

        public string? AssemblyId { get; set; }
        
        public override IEnumerable<IDocItem>? Items => Types;

        public override string ToDeclareCSharpCode() => $"namespace {DisplayName};";
    }

    public abstract class TypeDocItem : DocItem
    {
        public List<IMemberDocItem>? Members { get; set; }

        public string? AssemblyId { get; set; }
        
        public string? NamespaceId { get; set; }
        
        public override IEnumerable<IDocItem>? Items => Members;
        
        public string? BaseTypeId { get; set; }
        
        public IEnumerable<string>? InterfaceIds { get; set; }

    }

    public class ClassDocItem : TypeDocItem, IHaveTypeParameters
    {
        public List<TypeParameterDocItem>? TypeParameters { get; set; }
        
        public bool IsSealed { get; set; }
        
        public bool IsAbstract { get; set; }
        
        public bool IsStatic { get; set; }

        public override string ToDeclareCSharpCode() =>
            $"{Accessiblity.ToCSharpText()} {(IsStatic ? "static " : string.Empty)}{(IsSealed ? "sealed " : string.Empty)}{(IsAbstract ? "abstract " : string.Empty)}class {DisplayName};";
    }

    public class InterfaceDocItem : TypeDocItem, IHaveTypeParameters
    {
        public List<TypeParameterDocItem>? TypeParameters { get; set; }
        
        public override string ToDeclareCSharpCode() =>
            $"{Accessiblity.ToCSharpText()} interface {DisplayName};";

    }

    public class EnumDocItem : TypeDocItem
    {
        public override string ToDeclareCSharpCode() =>
            $"{Accessiblity.ToCSharpText()} enum {DisplayName};";
    }

    public class StructDocItem : TypeDocItem, IHaveTypeParameters
    {
        public List<TypeParameterDocItem>? TypeParameters { get; set; }
        
        public override string ToDeclareCSharpCode() =>
            $"{Accessiblity.ToCSharpText()} struct {DisplayName};";
    }

    public class DelegateDocItem : TypeDocItem, IHaveTypeParameters, IHaveParameters, IHaveReturnValue
    {
        public List<TypeParameterDocItem>? TypeParameters { get; set; }

        public List<ParameterDocItem>? Parameters { get; set; }

        public ReturnDocItem? ReturnValue { get; set; }
        
        public override string ToDeclareCSharpCode() =>
            $"{Accessiblity.ToCSharpText()} delegate {ReturnValue?.ToDeclareCSharpCode() ?? "void"} {DisplayName}({string.Join(", ", Parameters.OrEmpty().Select(p => p.ToDeclareCSharpCode()))});";

    }

    public interface IMemberDocItem : IDocItem
    {
        string? AssemblyId { get; }

        string? NamespaceId { get; }

        string? TypeId { get; }
    }
    
    
    public abstract class MemberDocItem : DocItem, IMemberDocItem
    {
        public string? AssemblyId { get; set; }

        public string? NamespaceId { get; set; }

        public string? TypeId { get; set; }
    }

    public class ConstructorDocItem : MemberDocItem, IHaveParameters
    {
        public List<ParameterDocItem>? Parameters { get; set; }

        public override IEnumerable<IDocItem>? Items => Parameters;
        
        public string CSharpConstructorName { get; set; }
        
        public override string ToDeclareCSharpCode()
        {
            var paramsText = Parameters.OrEmpty().Select(p => p.ToDeclareCSharpCode())
                .ConcatWith(" ,");

            return $"{Accessiblity.ToCSharpText()} {CSharpConstructorName}({paramsText});";
        }
    }

    public class FieldDocItem : MemberDocItem
    {
        public TypeInfo TypeInfo { get; set; }
        
        public bool IsStatic { get; set; }
        
        public bool IsReadOnly { get; set; }
        
        public object? ConstantValue { get; set; }
        
        public bool IsConstant { get; set; }
        
        public bool IsVolatile { get; set; }

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
    
    public class PropertyDocItem : MemberDocItem
    {
        public TypeInfo TypeInfo { get; set; }
        
        public bool HasGet { get; set; }
        
        public bool HasSet { get; set; }
        
        public bool IsInit { get; set; }

        public bool IsStatic { get; set; }
        
        public bool IsOverride { get; set; }
        
        public bool IsVirtual { get; set; }
        
        public bool IsAbstract { get; set; }
        
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

    public class MethodDocItem : MemberDocItem, IHaveParameters, IHaveTypeParameters, IHaveReturnValue
    {
        public List<ParameterDocItem>? Parameters { get; set; }

        public override IEnumerable<IDocItem>? Items => Parameters;
        
        public List<TypeParameterDocItem>? TypeParameters { get; set; }
        
        public ReturnDocItem? ReturnValue { get; set; }
        
        public bool IsStatic { get; set; }
        
        public bool IsOverride { get; set; }
        
        public bool IsVirtual { get; set; }
        
        public bool IsAbstract { get; set; }
        
        public override string ToDeclareCSharpCode()
        {
            var modifiersText = new List<string>();
            if(IsAbstract) modifiersText.Add("abstract ");
            if(IsStatic) modifiersText.Add("static ");
            if(IsOverride) modifiersText.Add("override ");
            if(IsVirtual) modifiersText.Add("virtual ");

            var returnValueText = ReturnValue is not null ? ReturnValue.ToDeclareCSharpCode() : "void";
            var typeParamsText = TypeParameters.OrEmpty().Select(p => p.Name)
                .ConcatWith(" ,")
                .SurroundsWith("<", ">", v => !string.IsNullOrEmpty(v));
            var paramsText = Parameters.OrEmpty().Select(p => p.ToDeclareCSharpCode())
                .ConcatWith(" ,");

            return $"{Accessiblity.ToCSharpText()} {string.Join("", modifiersText)}{returnValueText} {Name}{typeParamsText}({paramsText});";
        }
    }

    public class EventDocItem : MemberDocItem
    {
        public override string ToDeclareCSharpCode() => string.Empty;
    }

    public class ParameterDocItem: DocItem
    {
        public TypeInfo TypeInfo { get; set; }
        public string? XmlDocText { get; set; }

        public ValueRefKind RefKind { get; set; } = ValueRefKind.None;

        public override string ToDeclareCSharpCode()
        {
            var refKindString = (RefKind == ValueRefKind.RefReadoly || RefKind == ValueRefKind.None)
                ? string.Empty
                : (RefKind.ToString().ToLower() + " ");
            return $"{refKindString}{TypeInfo.DisplayName} {Name}";
        }
    }
    
    public class ReturnDocItem: DocItem
    {
        public TypeInfo TypeInfo { get; set; }
        
        public string? XmlDocText { get; set; }

        public ValueRefKind RefKind { get; set; } = ValueRefKind.None;

        public override string ToDeclareCSharpCode()
        {
            var refKindString = RefKind == ValueRefKind.RefReadoly ? "ref readonly "
                : string.Empty;
            return $"{refKindString}{TypeInfo.DisplayName}";
        }
    }

    public class TypeParameterDocItem : DocItem
    {
        public string? XmlDocText { get; set; }
        
        public override string ToDeclareCSharpCode() => string.Empty;
    }
    
    public class OverloadMethodDocItem : IDocItem, IMemberDocItem
    {
        public OverloadMethodDocItem(IList<MethodDocItem> docItems)
        {
            Methods = docItems;
            var first = docItems.First();
            Id = first.Id;
            DisplayName = first.MetadataName;
            AssemblyId = first.AssemblyId;
            NamespaceId = first.NamespaceId;
            XmlDocInfo = first.XmlDocInfo;
            TypeId = first.TypeId;
        }

        public string? Id { get; }
        public string? DisplayName { get; }

        public string? AssemblyId { get; set; }

        public string? NamespaceId { get; set; }
        
        public string? TypeId { get; }

        public IEnumerable<IDocItem>? Items { get; } = new List<IDocItem>();
        
        public XmlDocInfo? XmlDocInfo { get; }
        
        public IEnumerable<MethodDocItem> Methods { get; }
    }
        
    public class OverloadConstructorDocItem : IDocItem, IMemberDocItem
    {
        public OverloadConstructorDocItem(IList<ConstructorDocItem> docItems)
        {
            var first = docItems.First();
            Id = first.Id;
            DisplayName = first.MetadataName;
            Constructors = docItems;
            AssemblyId = first.AssemblyId;
            NamespaceId = first.NamespaceId;
            TypeId = first.TypeId;
            XmlDocInfo = first.XmlDocInfo;
        }

        public string? Id { get; }
        public string? DisplayName { get; }
       

        public IEnumerable<IDocItem>? Items { get; } = new List<IDocItem>();
        
        public IEnumerable<ConstructorDocItem> Constructors { get; }
        
        public XmlDocInfo? XmlDocInfo { get; }
        
        public string? AssemblyId { get; }
        
        public string? NamespaceId { get; }
        
        public string? TypeId { get; }
    }

    public class TypeInfo
    {
        public string TypeId { get; set; }

        public string Name { get; set; }
        
        public string DisplayName { get; set; }
        
        public TypeInfo LinkType { get; set; }
    }

    public enum ValueRefKind
    {
        None,
        In,
        Out,
        Ref,
        RefReadoly
    }
}
