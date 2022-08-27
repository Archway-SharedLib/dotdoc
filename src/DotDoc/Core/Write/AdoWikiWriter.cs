using System.Collections.Immutable;
using System.Text;
using DotDoc.Core.Read;

namespace DotDoc.Core.Write
{
    public class AdoWikiWriter : IWriter, IFileSystemOperation
    {
        private readonly List<DocItem> _docItems;
        private readonly DotDocEngineOptions _options;
        private readonly IFsModel _fsModel;
        private readonly ILogger _logger;
        private readonly TextTransform _textTransform;
        private readonly DocItemContainer _docItemContainer;

        public AdoWikiWriter(IEnumerable<DocItem> docItems, DotDocEngineOptions options, IFsModel fsModel, ILogger logger)
        {
            _docItems = docItems?.ToList() ?? throw new ArgumentNullException(nameof(docItems));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _fsModel = fsModel ?? throw new ArgumentNullException(nameof(fsModel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _docItemContainer = new DocItemContainer(_docItems, logger);
            _textTransform = new TextTransform(_docItemContainer, this, _logger);
        }

        public async Task WriteAsync()
        {
            var rootDir = _fsModel.CreateDirectoryModel(_options.OutputDir);
            foreach (var assemDocItem in _docItems.OfType<AssemblyDocItem>())
            {
                await WriteAssemblyAsync(rootDir, assemDocItem);
            }
        }

        private async Task WriteAssemblyAsync(IDirectoryModel rootDir, AssemblyDocItem assemDocItem)
        {
            var safeName = SafeFileOrDirectoryName(assemDocItem.ToFileName());
            var assemDir = _fsModel.CreateDirectoryModel(_fsModel.PathJoin(rootDir.GetFullName(), safeName));

            var sb = new StringBuilder();
            AppendTitle(sb, "Assembly", assemDocItem.DisplayName);

            AppendItemList<NamespaceDocItem>("Namespaces", assemDocItem, sb);

            var file = _fsModel.CreateFileModel(_fsModel.PathJoin(rootDir.GetFullName(), safeName + ".md"));
            file.WriteText(sb.ToString());

            foreach (var nsDocItem in assemDocItem.Namespaces.OrEmpty())
            {
                await WriteNamespaceAsync(assemDir, nsDocItem);
            }
        }

        private async Task WriteNamespaceAsync(IDirectoryModel assemDir, NamespaceDocItem nsDocItem)
        {
            assemDir.CreateIfNotExists();
            
            var safeName = SafeFileOrDirectoryName(nsDocItem.ToFileName());
            var nsDir = _fsModel.CreateDirectoryModel(_fsModel.PathJoin(assemDir.GetFullName(), safeName));

            var sb = new StringBuilder();
            AppendTitle(sb, "Namespace", nsDocItem.DisplayName);
            AppendNamespaceAssemblyInformation(sb, nsDocItem.Id, nsDocItem.Id, nsDocItem.AssemblyId, true);

            AppendDeclareCode(sb, nsDocItem.ToDeclareCSharpCode());
            
            AppendItemList<ClassDocItem>("Classes", nsDocItem, sb);
            AppendItemList<StructDocItem>("Structs", nsDocItem, sb);
            AppendItemList<InterfaceDocItem>("Interfaces", nsDocItem, sb);
            AppendItemList<EnumDocItem>("Enums", nsDocItem, sb);
            AppendItemList<DelegateDocItem>("Delegates", nsDocItem, sb);

            var file = _fsModel.CreateFileModel(_fsModel.PathJoin(assemDir.GetFullName(), safeName + ".md"));
            file.WriteText(sb.ToString());
            // await File.WriteAllTextAsync(Path.Combine(rootDir.GetFullName(), safeName + ".md"), sb.ToString(), Encoding.UTF8);

            foreach (var typeDocItem in nsDocItem.Types.OrEmpty())
            {
                await WriteTypeAsync(nsDir, typeDocItem);
            }
        }

        private async Task WriteTypeAsync(IDirectoryModel nsDir, TypeDocItem typeDocItem)
        {
            nsDir.CreateIfNotExists();
            
            var sb = new StringBuilder();
            AppendTitle(sb, GetTypeTypeName(typeDocItem), typeDocItem.DisplayName);
            AppendNamespaceAssemblyInformation(sb, typeDocItem.Id, typeDocItem.NamespaceId, typeDocItem.AssemblyId, false);

            sb.AppendLine(_textTransform.ToMdText(typeDocItem, typeDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            
            AppendDeclareCode(sb, typeDocItem.ToDeclareCSharpCode());

            if (typeDocItem is not DelegateDocItem)
            {
                if (!string.IsNullOrEmpty(typeDocItem.BaseTypeId))
                {
                    var bases = new List<string>();
                    var baseId = typeDocItem.BaseTypeId;
                    while (!string.IsNullOrEmpty(baseId))
                    {
                        bases.Add(baseId);
                        var baseDocItem = _docItemContainer.Get(baseId);
                        baseId = null;
                        if (baseDocItem is TypeDocItem baseTypeDoc)
                        {
                            baseId = baseTypeDoc.BaseTypeId;
                        }
                    }

                    var links = bases.AsEnumerable().Reverse().Select(id => _textTransform.ToMdLink(typeDocItem, id));
                    sb.AppendLine("Inheritance: " + string.Join(" > ", links.Append(typeDocItem.DisplayName))).AppendLine();
                }
                if (typeDocItem.InterfaceIds.OrEmpty().Any())
                {
                    sb.AppendLine("Implements: " + string.Join(", ", typeDocItem.InterfaceIds.Select(id => _textTransform.ToMdLink(typeDocItem, id)))).AppendLine();    
                }
            }
            
            AppendItemList<ConstructorDocItem>("Constructors", typeDocItem, sb);
            AppendItemList<MethodDocItem>("Methods", typeDocItem, sb);
            AppendItemList<PropertyDocItem>("Properties", typeDocItem, sb);
            AppendFieldItemList(typeDocItem, sb);
            AppendItemList<EventDocItem>("Events", typeDocItem, sb);
            
            if(typeDocItem is IHaveTypeParameters htp)
                AppendTypeParameterList(htp.TypeParameters.OrEmpty(), typeDocItem, sb);
            if(typeDocItem is IHaveParameters hp)
                AppendParameterList(hp.Parameters.OrEmpty(), typeDocItem, sb);
            if(typeDocItem is IHaveReturnValue hrv)
                AppendReturnValue(hrv, typeDocItem, sb);
            
            var typeDirOrFile = _fsModel.PathJoin(nsDir.GetFullName(), SafeFileOrDirectoryName(typeDocItem.ToFileName()));
            _fsModel.CreateFileModel(typeDirOrFile + ".md").WriteText(sb.ToString());
            //await File.WriteAllTextAsync(typeDirOrFile + ".md", sb.ToString(), Encoding.UTF8);

            if (typeDocItem is EnumDocItem) return;
            
            var typeDir = _fsModel.CreateDirectoryModel(typeDirOrFile);

            foreach (var memberDocItem in PrepareMemberDocItems(typeDocItem.Members.OrEmpty()))
            {
                await WriteMemberAsync(typeDir, typeDocItem, memberDocItem);
            }
        }

        private IEnumerable<MemberDocItem> PrepareMemberDocItems(IEnumerable<MemberDocItem> members)
        {
            var nonMethodDocItems = new List<MemberDocItem>();
            var methodDocItems = new Dictionary<string, OverloadMethodDocItem>();
            var overloadConstructorItem = new OverloadConstructorDocItem();
            
            foreach (var member in members)
            {
                if (member is MethodDocItem method)
                {
                    var name = member.Name!;
                    var overload = methodDocItems.ContainsKey(name) ? methodDocItems[name] : new();
                    overload.Name = name;
                    overload.DisplayName = name;
                    overload.AssemblyId = method.AssemblyId;
                    overload.NamespaceId = method.NamespaceId;
                    overload.TypeId = method.TypeId;
                    overload.Methods.Add(method);
                    methodDocItems[name] = overload;
                }
                else if (member is ConstructorDocItem ctor)
                {
                    overloadConstructorItem.Name = ctor.Name;
                    overloadConstructorItem.DisplayName = ctor.Name;
                    overloadConstructorItem.AssemblyId = ctor.AssemblyId;
                    overloadConstructorItem.NamespaceId = ctor.NamespaceId;
                    overloadConstructorItem.TypeId = ctor.TypeId;
                    overloadConstructorItem.Constructors.Add(ctor);
                }
                else
                {
                    nonMethodDocItems.Add(member);
                }
            }

            if (overloadConstructorItem.Constructors.Any())
            {
                nonMethodDocItems.Add(overloadConstructorItem);
            }

            foreach (var method in methodDocItems.Values)
            {
                if (method.Methods.Count == 1)
                {
                    nonMethodDocItems.Add(method.Methods.First());
                }
                else
                {
                    nonMethodDocItems.Add(method);
                }
            }

            return nonMethodDocItems;
        }

        private async Task WriteMemberAsync(IDirectoryModel typeDir, TypeDocItem typeDocItem, MemberDocItem memberDocItem)
        {
            var sb = memberDocItem switch
            {
                OverloadConstructorDocItem conDocItem => CreteConstructorPageText(conDocItem),
                MethodDocItem methodDocItem => CreteMethodPageText(methodDocItem),
                PropertyDocItem propDocItm => CretePropertyPageText(propDocItm),
                FieldDocItem fieldDocItm => CreteFieldPageText(fieldDocItm),
                OverloadMethodDocItem oMethodDocItem => CreteOverloadMethodPageText(oMethodDocItem),
                _ => null
            };
            if (sb is null) return;

            typeDir.CreateIfNotExists();
            _fsModel.CreateFileModel(_fsModel.PathJoin(typeDir.GetFullName(), SafeFileOrDirectoryName(memberDocItem.ToFileName())) + ".md").WriteText(sb.ToString());
            // await File.WriteAllTextAsync(Path.Combine(typeDir.FullName, SafeFileOrDirectoryName(memberDocItem.DisplayName)) + ".md", sb.ToString(), Encoding.UTF8);
        }

        private StringBuilder CreteConstructorPageText(OverloadConstructorDocItem memberDocItem)
        {
            var sb = new StringBuilder();
            AppendTitle(sb, "Constructor", _docItemContainer.Get(memberDocItem.TypeId).DisplayName);
            
            AppendNamespaceAssemblyInformation(sb, memberDocItem.Constructors.First().Id, memberDocItem.NamespaceId, memberDocItem.AssemblyId, false);
            
            if (memberDocItem.Constructors.Count == 1)
            {
                var ctor = memberDocItem.Constructors.First();
                sb.AppendLine(_textTransform.ToMdText(ctor, ctor, t => t.XmlDocInfo?.Summary)).AppendLine();

                AppendDeclareCode(sb, ctor.ToDeclareCSharpCode());

                AppendParameterList(ctor.Parameters.OrEmpty(), ctor, sb);
                return sb;    
            }

            sb.AppendLine(_textTransform.ToMdText(memberDocItem, memberDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            
            AppendTitle(sb, "Overloads", 2);

            sb.AppendLine("| Name | Summary |");
            sb.AppendLine("|------|---------|");

            foreach (var childItem in memberDocItem.Constructors)
            {
                var nameCellValue = 
                    $"{_textTransform.EscapeMdText(childItem.DisplayName)}";

                sb.AppendLine($@"| {nameCellValue} | {_textTransform.ToMdText(childItem, childItem, t => t.XmlDocInfo?.Summary, true)} |");
            }

            sb.AppendLine();
            
            foreach (var childItem in memberDocItem.Constructors)
            {
                AppendTitle(sb, childItem.DisplayName, 2);
                
                sb.AppendLine(_textTransform.ToMdText(childItem, childItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            
                AppendDeclareCode(sb, childItem.ToDeclareCSharpCode());
                AppendParameterList(childItem.Parameters.OrEmpty(), memberDocItem, sb, 3);
            }

            return sb;
        }
        
        private StringBuilder CreteMethodPageText(MethodDocItem memberDocItem)
        {
            var sb = new StringBuilder();
            AppendTitle(sb, "Method", memberDocItem.DisplayName);
            AppendNamespaceAssemblyInformation(sb, memberDocItem.Id, memberDocItem.NamespaceId, memberDocItem.AssemblyId, false);

            sb.AppendLine(_textTransform.ToMdText(memberDocItem, memberDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            
            AppendDeclareCode(sb, memberDocItem.ToDeclareCSharpCode());
            
            AppendTypeParameterList(memberDocItem.TypeParameters.OrEmpty(), memberDocItem, sb);
            AppendParameterList(memberDocItem.Parameters.OrEmpty(), memberDocItem, sb);
            AppendReturnValue(memberDocItem, memberDocItem, sb);

            return sb;
        }
        
        private StringBuilder CreteOverloadMethodPageText(OverloadMethodDocItem overloadMethodDocItem)
        {
            var sb = new StringBuilder();
            AppendTitle(sb, "Method", overloadMethodDocItem.DisplayName);
            AppendNamespaceAssemblyInformation(sb, 
                overloadMethodDocItem.Methods.First().Id, // 先頭のIDを利用 
                overloadMethodDocItem.NamespaceId, 
                overloadMethodDocItem.AssemblyId, 
                false);

            sb.AppendLine(_textTransform.ToMdText(overloadMethodDocItem, overloadMethodDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            
            AppendTitle(sb, "Overloads", 2);
            
            sb.AppendLine("| Name | Summary |");
            sb.AppendLine("|------|---------|");

            foreach (var childItem in overloadMethodDocItem.Methods)
            {
                var nameCellValue = 
                    $"{_textTransform.EscapeMdText(childItem.DisplayName)}";

                sb.AppendLine($@"| {nameCellValue} | {_textTransform.ToMdText(childItem, childItem, t => t.XmlDocInfo?.Summary, true)} |");
            }

            foreach (var memberDocItem in overloadMethodDocItem.Methods)
            {
                AppendTitle(sb, memberDocItem.DisplayName, 2);
                
                sb.AppendLine(_textTransform.ToMdText(memberDocItem, memberDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            
                AppendDeclareCode(sb, memberDocItem.ToDeclareCSharpCode());
            
                AppendTypeParameterList(memberDocItem.TypeParameters.OrEmpty(), memberDocItem, sb, 3);
                AppendParameterList(memberDocItem.Parameters.OrEmpty(), memberDocItem, sb, 3);
                AppendReturnValue(memberDocItem, memberDocItem, sb, 3);
            }

            return sb;
        }

        private void AppendTitle(StringBuilder sb, string title, int depth = 1) =>
            sb.AppendLine($"{string.Concat(Enumerable.Repeat("#", depth))} {_textTransform.EscapeMdText(title)}").AppendLine();
        
        private StringBuilder CretePropertyPageText(PropertyDocItem memberDocItem)
        {
            var sb = new StringBuilder();
            AppendTitle(sb, "Property", memberDocItem.DisplayName);
            AppendNamespaceAssemblyInformation(sb, memberDocItem.Id, memberDocItem.NamespaceId, memberDocItem.AssemblyId, false);

            sb.AppendLine(_textTransform.ToMdText(memberDocItem, memberDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            
            AppendDeclareCode(sb, memberDocItem.ToDeclareCSharpCode());
            
            AppendTitle(sb, "Property Value", 2);

            sb.AppendLine($"{_textTransform.ToMdLink(memberDocItem, memberDocItem.TypeInfo.TypeId, memberDocItem.TypeInfo.DisplayName)}").AppendLine();
            if (!string.IsNullOrEmpty(memberDocItem.XmlDocInfo?.Value))
            {
                sb.AppendLine(_textTransform.ToMdText(memberDocItem, memberDocItem, t => t.XmlDocInfo?.Value)).AppendLine();
            }
            
            return sb;
        }
        
        private StringBuilder CreteFieldPageText(FieldDocItem memberDocItem)
        {
            var sb = new StringBuilder();
            AppendTitle(sb, "Field", memberDocItem.DisplayName);
            AppendNamespaceAssemblyInformation(sb, memberDocItem.Id, memberDocItem.NamespaceId, memberDocItem.AssemblyId, false);

            sb.AppendLine(_textTransform.ToMdText(memberDocItem, memberDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            
            AppendDeclareCode(sb, memberDocItem.ToDeclareCSharpCode());
            
            AppendTitle(sb, "Field Value", 2);

            sb.AppendLine($"{_textTransform.ToMdLink(memberDocItem, memberDocItem.TypeInfo.GetLinkTypeInfo().TypeId, memberDocItem.TypeInfo.GetLinkTypeInfo().DisplayName)}").AppendLine();
            if (!string.IsNullOrEmpty(memberDocItem.XmlDocInfo?.Value))
            {
                sb.AppendLine(_textTransform.ToMdText(memberDocItem, memberDocItem, t => t.XmlDocInfo?.Value)).AppendLine();
            }
            
            return sb;
        }

        private void AppendTitle(StringBuilder sb, string type, string title) =>
            sb.AppendLine($"# {_textTransform.EscapeMdText(title)} {_textTransform.EscapeMdText(type)}").AppendLine();
        
        private void AppendDeclareCode(StringBuilder sb, string code)
        {
            sb.AppendLine("```csharp");
            sb.AppendLine(code);
            sb.AppendLine("```");
            sb.AppendLine();
        }
        
        private void AppendNamespaceAssemblyInformation(StringBuilder sb, string targetId, string namespaceId, string assemblyId, bool withoutNamespace)
        {
            var targetItem = _docItemContainer.Get(targetId);
            var nsDocItem = _docItemContainer.Get(namespaceId);
            var assemDocItem = _docItemContainer.Get(assemblyId);
            if (!withoutNamespace)
            {
                sb.AppendLine($"namespace: [{_textTransform.EscapeMdText(nsDocItem?.DisplayName)}]({GetRelativeLink(targetItem, nsDocItem)})<br />");
            }
            sb.AppendLine($"assembly: [{_textTransform.EscapeMdText(assemDocItem?.DisplayName)}]({GetRelativeLink(targetItem, assemDocItem)})").AppendLine();
        }

        private void AppendItemList<T>(string title, DocItem docItem, StringBuilder sb) where T : DocItem
        {
            var docItems = docItem.Items.OrEmpty().OfType<T>();
            if (!docItems.Any()) return;

            AppendTitle(sb, title, 2);

            sb.AppendLine("| Name | Summary |");
            sb.AppendLine("|------|---------|");

            foreach (var childItem in docItems)
            {
                var nameCellValue = 
                    $"[{_textTransform.EscapeMdText(childItem.DisplayName)}]({GetRelativeLink(docItem, childItem)})";

               sb.AppendLine($@"| {nameCellValue} | {_textTransform.ToMdText(docItem, childItem, t => t.XmlDocInfo?.Summary, true)} |");
            }

            sb.AppendLine();
        }

        private void AppendFieldItemList(DocItem docItem, StringBuilder sb)
        {
            var docItems = docItem.Items.OrEmpty().OfType<FieldDocItem>();
            if (!docItems.Any()) return;

            AppendTitle(sb,"Fields", 2);

            sb.AppendLine("| Name | Value | Summary |");
            sb.AppendLine("|------|-------|---------|");

            var isEnumField = docItem is EnumDocItem;
           
            foreach (var childItem in docItems)
            {
                var nameCellValue = isEnumField ?
                    _textTransform.EscapeMdText(childItem.DisplayName) :
                    $"[{_textTransform.EscapeMdText(childItem.DisplayName)}]({GetRelativeLink(docItem, childItem)})";

                sb.AppendLine($@"| {nameCellValue} | {GetConstantValueDisplayText(childItem.IsConstant, childItem.ConstantValue) } | {_textTransform.ToMdText(docItem, childItem, t => t.XmlDocInfo?.Summary, true)} |");
            }

            sb.AppendLine();
        }

        private string GetConstantValueDisplayText(bool isConst, object? value)
        {
            if (!isConst) return string.Empty;
            if (value is null) return "null";
            var text = value is char ? $"'{value}'" :
                value is string ? $"\"{value}\"" : value.ToString();
            return _textTransform.EscapeMdText(text);
        }

        
        private void AppendParameterList(IEnumerable<ParameterDocItem> parameters, DocItem source, StringBuilder sb, int depth = 2)
        {
            if(parameters.Any())
            {
                AppendTitle(sb, "Parameters", depth);
                sb.AppendLine("| Type | Name | Summary |");
                sb.AppendLine("|------|------|---------|");
                foreach(var param in parameters)
                {
                    sb.AppendLine($@"| {_textTransform.ToMdLink(source,  param.TypeInfo.TypeId, param.TypeInfo.DisplayName)} | {_textTransform.EscapeMdText(param.DisplayName)} | {_textTransform.ToMdText(source, param, t => t.XmlDocText, true)} |");
                }
                sb.AppendLine();
            }
        }
        
        private void AppendTypeParameterList(IEnumerable<TypeParameterDocItem> typeParameters, DocItem source, StringBuilder sb, int depth = 2)
        {
            if(typeParameters.Any())
            {
                AppendTitle(sb,"Type Parameters", depth);
                sb.AppendLine("| Name | Summary |");
                sb.AppendLine("|------|---------|");
                foreach(var param in typeParameters)
                {
                    sb.AppendLine($@"| {_textTransform.EscapeMdText(param.DisplayName)} | {_textTransform.ToMdText(source, param, t => t.XmlDocText, true)} |");
                }
                sb.AppendLine();
            }
        }
        
        private void AppendReturnValue(IHaveReturnValue docItem, DocItem source, StringBuilder sb, int depth = 2)
        {
            if (docItem.ReturnValue?.TypeInfo is null) return;
            
            AppendTitle(sb, "Return Value", depth);
            sb.AppendLine(_textTransform.ToMdLink(source,  docItem.ReturnValue.TypeInfo.GetLinkTypeInfo().TypeId, docItem.ReturnValue.DisplayName)).AppendLine();
            sb.AppendLine(_textTransform.ToMdText(source, source, t => t.XmlDocInfo?.Returns)).AppendLine();
        }
        

        public string GetRelativeLink(DocItem source, DocItem dest)
        {
            var basePath = dest switch
            {
                AssemblyDocItem => $"{SafeFileOrDirectoryName(dest.ToFileName())}.md",
                NamespaceDocItem n => $"{SafeFileOrDirectoryName(_docItemContainer.Get(n.AssemblyId).ToFileName())}/{SafeFileOrDirectoryName(dest.ToFileName())}.md",
                TypeDocItem t => $"{SafeFileOrDirectoryName(_docItemContainer.Get(t.AssemblyId).ToFileName())}/{SafeFileOrDirectoryName(_docItemContainer.Get(t.NamespaceId).ToFileName())}/{SafeFileOrDirectoryName(dest.ToFileName())}.md",
                MemberDocItem m => $"{SafeFileOrDirectoryName(_docItemContainer.Get(m.AssemblyId).ToFileName())}/{SafeFileOrDirectoryName(_docItemContainer.Get(m.NamespaceId).ToFileName())}/{SafeFileOrDirectoryName(_docItemContainer.Get(m.TypeId).ToFileName())}/{SafeFileOrDirectoryName(dest.ToFileName())}.md",
                _ => string.Empty,
            };
            return source switch
            {
                AssemblyDocItem => "./",
                NamespaceDocItem => "../",
                TypeDocItem => "../../",
                MemberDocItem => "../../../",
                _ => string.Empty,
            } + basePath;
        }

        public string SafeFileOrDirectoryName(string? fileOrDirectoryName)
            => (fileOrDirectoryName ?? string.Empty)
                .Replace(":", "%3A")
                .Replace("<", "%3C")
                .Replace(">", "%3E")
                .Replace("*", "%2A")
                .Replace("?", "%3F")
                .Replace("|", "%7C")
                .Replace("-", "%2D")
                .Replace("\"", "%22")
                .Replace(" ", "-");

        private string GetTypeTypeName(TypeDocItem docItem) =>
            docItem is ClassDocItem ? "Class" :
            docItem is InterfaceDocItem ? "Interface" :
            docItem is EnumDocItem ? "Enum" :
            docItem is StructDocItem ? "Struct" : string.Empty;
        
    }
    
    public class OverloadMethodDocItem : MemberDocItem
    {
        public List<MethodDocItem> Methods { get; } = new();
        public override string ToDeclareCSharpCode() => string.Empty;
    }
        
    public class OverloadConstructorDocItem : ConstructorDocItem
    {
        public List<ConstructorDocItem> Constructors { get; } = new();
        public override string ToDeclareCSharpCode() => string.Empty;
    }
}
