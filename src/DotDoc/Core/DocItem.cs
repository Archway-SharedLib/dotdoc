using DotDoc.Core.Read;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core
{
    public interface IDocItem
    {
        string? Id { get; }
        
        string? DisplayName { get; }
        
        IEnumerable<IDocItem>? Items { get; }
        
        XmlDocInfo? XmlDocInfo { get; }
    }

    public abstract class DocItem: IDocItem
    {
        private readonly ISymbol _symbol;

        protected DocItem(ISymbol symbol)
        {
            _symbol = symbol;
            Id = SymbolUtil.GetSymbolId(symbol);
            Name = symbol.Name;
            DisplayName = symbol.ToDisplayString();
            MetadataName = symbol.MetadataName;
            XmlDocInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml());
            Accessiblity = SymbolUtil.MapAccessibility(symbol.DeclaredAccessibility);
        }

        public string? Id { get; protected set; }

        public string? Name { get; protected set; }

        public string? DisplayName { get; protected set; }
        
        public string? MetadataName { get; protected set; }

        public XmlDocInfo? XmlDocInfo { get; protected set; }

        public virtual IEnumerable<IDocItem> Items { get; } = Enumerable.Empty<IDocItem>();

        public Accessibility Accessiblity { get; protected set; } = Accessibility.Unknown;

        public abstract string ToDeclareCSharpCode();

    }

    public class AssemblyDocItem : DocItem
    {
        public AssemblyDocItem(IAssemblySymbol symbol) : base(symbol)
        {
            DisplayName = symbol.Name;
        }

        public List<NamespaceDocItem> Namespaces { get; } = new();

        public override IEnumerable<IDocItem> Items => Namespaces;

        public override string ToDeclareCSharpCode() => string.Empty;
    }

    public class NamespaceDocItem : DocItem
    {
        public NamespaceDocItem(INamespaceSymbol symbol) : base(symbol)
        {
            AssemblyId = SymbolUtil.GetSymbolId(symbol.ContainingAssembly);
        }

        public List<TypeDocItem> Types { get; } = new();

        public string? AssemblyId { get;}
        
        public override IEnumerable<IDocItem> Items => Types;

        public override string ToDeclareCSharpCode() => $"namespace {DisplayName};";
    }

    public abstract class TypeDocItem : DocItem
    {
        public TypeDocItem(INamedTypeSymbol symbol) : base(symbol)
        {
            DisplayName = symbol.ToDisplayString()
                .Substring(symbol.ContainingNamespace.ToDisplayString().Length + 1);
            NamespaceId = SymbolUtil.GetSymbolId(symbol.ContainingNamespace);
            AssemblyId = SymbolUtil.GetSymbolId(symbol.ContainingAssembly);
            BaseType = symbol.BaseType is not null ? symbol.BaseType.ToTypeInfo() : null;
            Interfaces = symbol.Interfaces.OrEmpty().Select(i => i.ToTypeInfo()).ToList();
        }

        public List<IMemberDocItem> Members { get; } = new();

        public string? AssemblyId { get; protected set; }
        
        public string? NamespaceId { get; protected set; }
        
        public override IEnumerable<IDocItem> Items => Members;
        
        public TypeInfo BaseType { get; protected set; }

        public List<TypeInfo> Interfaces { get; }

    }

    public class ClassDocItem : TypeDocItem
    {
        public ClassDocItem(INamedTypeSymbol symbol) : base(symbol)
        {
            IsSealed = symbol.IsSealed;
            IsAbstract = symbol.IsAbstract;
            IsStatic = symbol.IsStatic;
            TypeParameters.AddRange( SymbolUtil.RetrieveTypeParameters(symbol.TypeParameters, XmlDocInfo));
        }

        public List<TypeParameterDocItem> TypeParameters { get; } = new ();
        
        public bool IsSealed { get; }
        
        public bool IsAbstract { get; }
        
        public bool IsStatic { get; }

        public override string ToDeclareCSharpCode() =>
            $"{Accessiblity.ToCSharpText()} {(IsStatic ? "static " : string.Empty)}{(IsSealed ? "sealed " : string.Empty)}{(IsAbstract ? "abstract " : string.Empty)}class {DisplayName};";
    }

    public class InterfaceDocItem : TypeDocItem
    {
        public InterfaceDocItem(INamedTypeSymbol symbol) : base(symbol)
        {
            TypeParameters.AddRange( SymbolUtil.RetrieveTypeParameters(symbol.TypeParameters, XmlDocInfo));
        }

        public List<TypeParameterDocItem> TypeParameters { get; } = new();
        
        public override string ToDeclareCSharpCode() =>
            $"{Accessiblity.ToCSharpText()} interface {DisplayName};";

    }

    public class EnumDocItem : TypeDocItem
    {
        public EnumDocItem(INamedTypeSymbol symbol) : base(symbol)
        {
        }
        public override string ToDeclareCSharpCode() =>
            $"{Accessiblity.ToCSharpText()} enum {DisplayName};";
    }

    public class StructDocItem : TypeDocItem
    {
        public StructDocItem(INamedTypeSymbol symbol) : base(symbol)
        {
            TypeParameters.AddRange( SymbolUtil.RetrieveTypeParameters(symbol.TypeParameters, XmlDocInfo));
        }

        public List<TypeParameterDocItem> TypeParameters { get; } = new();
        
        public override string ToDeclareCSharpCode() =>
            $"{Accessiblity.ToCSharpText()} struct {DisplayName};";
    }

    public class DelegateDocItem : TypeDocItem
    {
        public DelegateDocItem(INamedTypeSymbol symbol) : base(symbol)
        {
            TypeParameters.AddRange( SymbolUtil.RetrieveTypeParameters(symbol.TypeParameters, XmlDocInfo));
            var delegMethod = symbol.DelegateInvokeMethod!;
            Parameters.AddRange(SymbolUtil.RetrieveParameters(delegMethod.Parameters, XmlDocInfo));
            ReturnValue = delegMethod.ReturnsVoid
                ? null
                : new ReturnItem(delegMethod);
        }

        public List<TypeParameterDocItem> TypeParameters { get; } = new();

        public List<ParameterDocItem> Parameters { get; } = new();

        public ReturnItem? ReturnValue { get; }
        
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
        protected MemberDocItem(ISymbol symbol) : base(symbol)
        {
            NamespaceId = SymbolUtil.GetSymbolId(symbol.ContainingNamespace);
            AssemblyId = SymbolUtil.GetSymbolId(symbol.ContainingAssembly);
            TypeId = SymbolUtil.GetSymbolId(symbol.ContainingType);
            DisplayName = symbol.ToDisplayString().Substring(symbol.ContainingType.ToDisplayString().Length + 1);
        }
        
        public string? AssemblyId { get; protected set; }

        public string? NamespaceId { get; protected set; }

        public string? TypeId { get; protected set; }
    }

    public class ConstructorDocItem : MemberDocItem
    {
        public ConstructorDocItem(IMethodSymbol symbol) : base(symbol)
        {
            CSharpConstructorName = symbol.ContainingType.Name;
            Parameters.AddRange(SymbolUtil.RetrieveParameters(symbol.Parameters, XmlDocInfo));
        }

        public List<ParameterDocItem> Parameters { get; } = new();

        public override IEnumerable<IDocItem> Items => Parameters;
        
        public string CSharpConstructorName { get; }
        
        public override string ToDeclareCSharpCode()
        {
            var paramsText = Parameters.OrEmpty().Select(p => p.ToDeclareCSharpCode())
                .ConcatWith(" ,");

            return $"{Accessiblity.ToCSharpText()} {CSharpConstructorName}({paramsText});";
        }
    }

    public class FieldDocItem : MemberDocItem
    {
        public FieldDocItem(IFieldSymbol symbol) : base(symbol)
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
    
    public class PropertyDocItem : MemberDocItem
    {
        public PropertyDocItem(IPropertySymbol symbol) : base(symbol)
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

    public class MethodDocItem : MemberDocItem
    {
        public MethodDocItem(IMethodSymbol symbol) : base(symbol)
        {
            ReturnValue = symbol.ReturnsVoid
                ? null
                : new ReturnItem(symbol);
            IsStatic = symbol.IsStatic;
            IsAbstract = symbol.IsAbstract;
            IsOverride = symbol.IsOverride;
            IsVirtual = symbol.IsVirtual;
            
            TypeParameters.AddRange( SymbolUtil.RetrieveTypeParameters(symbol.TypeParameters, XmlDocInfo));
            Parameters.AddRange(SymbolUtil.RetrieveParameters(symbol.Parameters, XmlDocInfo));
        }

        public List<ParameterDocItem> Parameters { get; } = new();

        public override IEnumerable<IDocItem> Items => Parameters;

        public List<TypeParameterDocItem> TypeParameters { get; } = new();
        
        public ReturnItem? ReturnValue { get; }
        
        public bool IsStatic { get; }
        
        public bool IsOverride { get; }
        
        public bool IsVirtual { get; }
        
        public bool IsAbstract { get; }
        
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
        public EventDocItem(IEventSymbol symbol) : base(symbol)
        {
        }
        
        public override string ToDeclareCSharpCode() => string.Empty;
    }

    public class ParameterDocItem: DocItem
    {
        public ParameterDocItem(IParameterSymbol symbol, XmlDocInfo docInfo) : base(symbol)
        {
            DisplayName = symbol.Name;
            TypeInfo = symbol.Type.ToTypeInfo();
            XmlDocText = docInfo?.Parameters.OrEmpty().FirstOrDefault(p => p.Name == symbol.Name)?.Text;
            RefKind = symbol.RefKind == Microsoft.CodeAnalysis.RefKind.In ? ValueRefKind.In :
                symbol.RefKind == Microsoft.CodeAnalysis.RefKind.Out ? ValueRefKind.Out :
                symbol.RefKind == Microsoft.CodeAnalysis.RefKind.None ? ValueRefKind.None :
                symbol.RefKind == Microsoft.CodeAnalysis.RefKind.Ref ? ValueRefKind.Ref : ValueRefKind.RefReadoly;
        }
        
        public TypeInfo TypeInfo { get; }
        public string? XmlDocText { get; }

        public ValueRefKind RefKind { get; }

        public override string ToDeclareCSharpCode()
        {
            var refKindString = (RefKind == ValueRefKind.RefReadoly || RefKind == ValueRefKind.None)
                ? string.Empty
                : (RefKind.ToString().ToLower() + " ");
            return $"{refKindString}{TypeInfo.DisplayName} {Name}";
        }
    }
    
    public class ReturnItem
    {
        public ReturnItem(IMethodSymbol symbol)
        {
            TypeInfo = symbol.ReturnType.ToTypeInfo();
            RefKind = symbol.ReturnsByRefReadonly ? ValueRefKind.RefReadoly : ValueRefKind.None;
        }
        
        public TypeInfo TypeInfo { get; }
        
        
        public string? XmlDocText { get; }

        public ValueRefKind RefKind { get; }

        public string ToDeclareCSharpCode()
        {
            var refKindString = RefKind == ValueRefKind.RefReadoly ? "ref readonly "
                : string.Empty;
            return $"{refKindString}{TypeInfo.DisplayName}";
        }
    }

    public class TypeParameterDocItem : DocItem
    {
        public TypeParameterDocItem(ITypeParameterSymbol symbol, XmlDocInfo docInfo) : base(symbol)
        {
            XmlDocText = docInfo?.TypeParameters.OrEmpty().FirstOrDefault(p => p.Name == symbol.Name)?.Text;
        }
        
        public string? XmlDocText { get; }
        
        public override string ToDeclareCSharpCode() => string.Empty;
    }
    
    public class OverloadMethodDocItem : IMemberDocItem
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
        
    public class OverloadConstructorDocItem : IMemberDocItem
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
        public TypeInfo(ITypeSymbol symbol)
        {
            TypeId = SymbolUtil.GetSymbolId(symbol);
            DisplayName = symbol.ToDisplayString();
            Name = symbol.Name;

            if (symbol is IArrayTypeSymbol arrayType)
            {
                LinkType = arrayType.ElementType.ToTypeInfo();
            } 
            else if (symbol is INamedTypeSymbol { IsGenericType: true } namedType)
                //namedType が Type{`N} の場合は型が束縛されていないとContainingType が Type{`N} になる
                // if (namedType.IsGenericType && namedType.TypeArguments[0].ContainingType is null)
            {
                if (namedType != namedType.OriginalDefinition)
                {
                    LinkType = namedType.OriginalDefinition.ToTypeInfo();
                    LinkType.DisplayName = namedType.ToDisplayString();
                }
            }

            if (symbol != symbol.BaseType)
            {
                BaseType = symbol.BaseType?.ToTypeInfo();    
            }
        }
        public string TypeId { get; }

        public string Name { get;  }
        
        public string DisplayName { get; private set; }

        public TypeInfo? LinkType { get; } = null;

        public TypeInfo? BaseType { get; } = null;
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
