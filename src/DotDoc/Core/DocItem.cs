using DotDoc.Core.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core
{
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
        public TypeInfo? ReturnValue { get; set; }
    }

    public abstract class DocItem
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public XmlDocInfo? XmlDocInfo { get; set; }

        public virtual IEnumerable<DocItem>? Items { get; } = Enumerable.Empty<DocItem>();

        public Accessibility Accessiblity { get; set; } = Accessibility.Unknown;

        public abstract string ToDeclareCSharpCode();

    }

    public class AssemblyDocItem : DocItem
    {
        public List<NamespaceDocItem>? Namespaces { get; set; }

        public override IEnumerable<DocItem>? Items => Namespaces;

        public override string ToDeclareCSharpCode() => string.Empty;
    }

    public class NamespaceDocItem : DocItem
    {
        public List<TypeDocItem>? Types { get; set; }

        public string? AssemblyId { get; set; }
        
        public override IEnumerable<DocItem>? Items => Types;

        public override string ToDeclareCSharpCode() => $"namespace {DisplayName};";
    }

    public abstract class TypeDocItem : DocItem
    {
        public List<MemberDocItem>? Members { get; set; }

        public string? AssemblyId { get; set; }
        
        public string? NamespaceId { get; set; }
        
        public override IEnumerable<DocItem>? Items => Members;

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

        public TypeInfo? ReturnValue { get; set; }
        
        public override string ToDeclareCSharpCode() =>
            $"{Accessiblity.ToCSharpText()} delegate {ReturnValue?.DisplayName ?? "void"} {DisplayName}({string.Join(", ", Parameters.Select(p => p.TypeInfo.DisplayName + " " + p.DisplayName))});";

    }

    public abstract class MemberDocItem : DocItem
    {
        public string? AssemblyId { get; set; }

        public string? NamespaceId { get; set; }

        public string? TypeId { get; set; }
    }

    public class ConstructorDocItem : MemberDocItem, IHaveParameters
    {
        public List<ParameterDocItem>? Parameters { get; set; }

        public override IEnumerable<DocItem>? Items => Parameters;
        
        public override string ToDeclareCSharpCode() => string.Empty;
    }

    public class FieldDocItem : MemberDocItem
    {
        public TypeInfo TypeInfo { get; set; }
        
        public override string ToDeclareCSharpCode() => string.Empty;
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

        public override IEnumerable<DocItem>? Items => Parameters;
        
        public List<TypeParameterDocItem>? TypeParameters { get; set; }
        
        public TypeInfo? ReturnValue { get; set; }
        
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

            var returnValueText = ReturnValue is not null ? ReturnValue.DisplayName : "void";
            var typeParamsText = TypeParameters.OrEmpty().Select(p => p.Name)
                .ConcatWith(" ,")
                .SurroundsWith("<", ">", v => !string.IsNullOrEmpty(v));
            var paramsText = Parameters.OrEmpty().Select(p => $"{p.TypeInfo.DisplayName} {p.Name}")
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
        
        public override string ToDeclareCSharpCode() => string.Empty;
    }

    public class TypeParameterDocItem : DocItem
    {
        public string? XmlDocText { get; set; }
        
        public override string ToDeclareCSharpCode() => string.Empty;
    }

    public class TypeInfo
    {
        public string TypeId { get; set; }

        public string Name { get; set; }
        
        public string DisplayName { get; set; }
    }
}
