using System.Collections.Immutable;
using System.Text;

namespace DotDoc.Core.Write
{
    internal class AdoWikiWriter : IWriter, IFileSystemOperation
    {
        private readonly List<DocItem> _docItems;
        private readonly DotDocEngineOptions _options;
        private readonly TextTransform _textTransform;
        private readonly ImmutableDictionary<string, DocItem> _flatItems;

        public AdoWikiWriter(IEnumerable<DocItem> docItems, DotDocEngineOptions options)
        {
            _docItems = docItems?.ToList() ?? throw new ArgumentNullException(nameof(docItems));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _flatItems = FlattenDocItems(ImmutableDictionary.CreateBuilder<string, DocItem>(), _docItems).ToImmutable();
            _textTransform = new TextTransform(_flatItems, this);
        }

        public async Task WriteAsync()
        {
            var rootDir = new DirectoryInfo(_options.OutputDir);
            foreach (var assemDocItem in _docItems.OfType<AssemblyDocItem>())
            {
                foreach (var nsDocItem in assemDocItem.Namespaces.OrEmpty())
                {
                    await WriteNamespaceAsync(rootDir, nsDocItem);
                }
            }
        }

        private async Task WriteNamespaceAsync(DirectoryInfo rootDir, NamespaceDocItem nsDocItem)
        {
            var safeName = SafeFileOrDirectoryName(nsDocItem.DisplayName);
            var nsDir = new DirectoryInfo(Path.Join(rootDir.FullName, safeName));

            var sb = new StringBuilder();
            AppendTitle(sb, "Namespace", nsDocItem.DisplayName);
            AppendNamespaceAssemblyInformation(sb, nsDocItem.Id, nsDocItem.Id, nsDocItem.AssemblyId, true);

            AppendItemList<ClassDocItem>("Classes", nsDocItem, sb);
            AppendItemList<StructDocItem>("Structs", nsDocItem, sb);
            AppendItemList<InterfaceDocItem>("Interfaces", nsDocItem, sb);
            AppendItemList<EnumDocItem>("Enums", nsDocItem, sb);
            AppendItemList<DelegateDocItem>("Delegates", nsDocItem, sb);

            await File.WriteAllTextAsync(Path.Combine(rootDir.FullName, safeName + ".md"), sb.ToString(), Encoding.UTF8);

            foreach (var typeDocItem in nsDocItem.Types.OrEmpty())
            {
                await WriteTypeAsync(nsDir, typeDocItem);
            }
        }

        private async Task WriteTypeAsync(DirectoryInfo nsDir, TypeDocItem typeDocItem)
        {
            if (!nsDir.Exists) nsDir.Create();

            var sb = new StringBuilder();
            AppendTitle(sb, GetTypeTypeName(typeDocItem), typeDocItem.DisplayName);
            AppendNamespaceAssemblyInformation(sb, typeDocItem.Id, typeDocItem.NamespaceId, typeDocItem.AssemblyId, false);

            sb.AppendLine(_textTransform.ToMdText(typeDocItem, typeDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();

            AppendItemList<ConstructorDocItem>("Constructors", typeDocItem, sb);
            AppendItemList<MethodDocItem>("Methods", typeDocItem, sb);
            AppendItemList<PropertyDocItem>("Properties", typeDocItem, sb);
            AppendItemList<FieldDocItem>("Fields", typeDocItem, sb);
            AppendItemList<EventDocItem>("Events", typeDocItem, sb);

            var typeDirOrFile = Path.Combine(nsDir.FullName, SafeFileOrDirectoryName(typeDocItem.DisplayName));
            await File.WriteAllTextAsync(typeDirOrFile + ".md", sb.ToString(), Encoding.UTF8);
            var typeDir = new DirectoryInfo(typeDirOrFile);

            foreach (var memberDocItem in typeDocItem.Members.OrEmpty())
            {
                await WriteMemberAsync(typeDir, typeDocItem, memberDocItem);
            }
        }

        private async Task WriteMemberAsync(DirectoryInfo typeDir, TypeDocItem typeDocItem, MemberDocItem memberDocItem)
        {
            var sb = memberDocItem switch
            {
                ConstructorDocItem conDocItem => CreteConstructorPageText(conDocItem),
                MethodDocItem methodDocItem => CreteMethodPageText(methodDocItem),
                PropertyDocItem propDocItm => CretePropertyPageText(propDocItm),
                FieldDocItem fieldDocItm => CreteFieldPageText(fieldDocItm),
                _ => null
            };
            if (sb is null) return;

            if (!typeDir.Exists) typeDir.Create();
            await File.WriteAllTextAsync(Path.Combine(typeDir.FullName, SafeFileOrDirectoryName(memberDocItem.DisplayName)) + ".md", sb.ToString(), Encoding.UTF8);
        }

        private StringBuilder CreteConstructorPageText(ConstructorDocItem memberDocItem)
        {
            var sb = new StringBuilder();
            AppendTitle(sb, "Constructor", memberDocItem.DisplayName);
            AppendNamespaceAssemblyInformation(sb, memberDocItem.Id, memberDocItem.NamespaceId, memberDocItem.AssemblyId, false);

            sb.AppendLine(_textTransform.ToMdText(memberDocItem, memberDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            AppendParameterList(memberDocItem.Parameters.OrEmpty(), memberDocItem, sb);
            
            return sb;
        }
        
        private StringBuilder CreteMethodPageText(MethodDocItem memberDocItem)
        {
            var sb = new StringBuilder();
            AppendTitle(sb, "Method", memberDocItem.DisplayName);
            AppendNamespaceAssemblyInformation(sb, memberDocItem.Id, memberDocItem.NamespaceId, memberDocItem.AssemblyId, false);

            sb.AppendLine(_textTransform.ToMdText(memberDocItem, memberDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            AppendParameterList(memberDocItem.Parameters.OrEmpty(), memberDocItem, sb);
            
            return sb;
        }
        
        private StringBuilder CretePropertyPageText(PropertyDocItem memberDocItem)
        {
            var sb = new StringBuilder();
            AppendTitle(sb, "Property", memberDocItem.DisplayName);
            AppendNamespaceAssemblyInformation(sb, memberDocItem.Id, memberDocItem.NamespaceId, memberDocItem.AssemblyId, false);

            sb.AppendLine(_textTransform.ToMdText(memberDocItem, memberDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            
            AppendSubTitle(sb, "Property Value");

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
            
            AppendSubTitle(sb, "Field Value");

            sb.AppendLine($"{_textTransform.ToMdLink(memberDocItem, memberDocItem.TypeInfo.TypeId, memberDocItem.TypeInfo.DisplayName)}").AppendLine();
            if (!string.IsNullOrEmpty(memberDocItem.XmlDocInfo?.Value))
            {
                sb.AppendLine(_textTransform.ToMdText(memberDocItem, memberDocItem, t => t.XmlDocInfo?.Value)).AppendLine();
            }
            
            return sb;
        }

        private void AppendTitle(StringBuilder sb, string type, string title) =>
            sb.AppendLine($"# {title} {type}").AppendLine();
        
        private void AppendSubTitle(StringBuilder sb, string title) =>
            sb.AppendLine($"## {title}").AppendLine();
        
        private void AppendNamespaceAssemblyInformation(StringBuilder sb, string targetId, string namespaceId, string assemblyId, bool withoutNamespace)
        {
            var targetItem = _flatItems[targetId];
            var nsDocItem = _flatItems[namespaceId];
            var assemDocItem = _flatItems[assemblyId];
            if (!withoutNamespace)
            {
                sb.AppendLine($"namespace: [{_textTransform.EscapeMdText(nsDocItem?.DisplayName)}]({GetRelativeLink(targetItem, nsDocItem)})<br />");
            }
            sb.AppendLine($"assembly: {_textTransform.EscapeMdText(assemDocItem?.DisplayName)}").AppendLine();
        }

        private void AppendItemList<T>(string title, DocItem docItem, StringBuilder sb) where T : DocItem
        {
            var docItems = docItem.Items.OrEmpty().OfType<T>();
            if (!docItems.Any()) return;

            AppendSubTitle(sb, title);
            sb.AppendLine("| Name | Summary |");
            sb.AppendLine("|------|---------|");

            var isEnumField = docItems.First() is FieldDocItem && docItem is EnumDocItem;
           
            foreach (var childItem in docItems)
            {
                var nameCellValue = isEnumField ?
                    _textTransform.EscapeMdText(childItem.DisplayName) :
                    $"[{_textTransform.EscapeMdText(childItem.DisplayName)}]({GetRelativeLink(docItem, childItem)})";

               sb.AppendLine($@"| {nameCellValue} | {_textTransform.ToMdText(docItem, childItem, t => t.XmlDocInfo?.Summary, true)} |");
            }

            sb.AppendLine();
        }

        private void AppendParameterList(IEnumerable<ParameterDocItem> parameters, DocItem source, StringBuilder sb)
        {
            if(parameters.Any())
            {
                AppendSubTitle(sb, "Parameters");
                sb.AppendLine("| Type | Name | Summary |");
                sb.AppendLine("|------|------|---------|");
                foreach(var param in parameters)
                {
                    sb.AppendLine($@"| {_textTransform.EscapeMdText(param.TypeInfo.DisplayName)} | {_textTransform.EscapeMdText(param.DisplayName)} | {_textTransform.ToMdText(source, param, t => t.XmlDocText, true)} |");
                }
            }
        }

        public string GetRelativeLink(DocItem source, DocItem dest)
        {
            var basePath = dest switch
            {
                NamespaceDocItem => $"{SafeFileOrDirectoryName(dest.DisplayName)}.md",
                TypeDocItem t => $"{SafeFileOrDirectoryName(_flatItems[t.NamespaceId].DisplayName)}/{SafeFileOrDirectoryName(dest.DisplayName)}.md",
                MemberDocItem m => $"{SafeFileOrDirectoryName(_flatItems[m.NamespaceId].DisplayName)}/{SafeFileOrDirectoryName(_flatItems[m.TypeId].DisplayName)}/{SafeFileOrDirectoryName(dest.DisplayName)}.md",
                _ => string.Empty,
            };
            return source switch
            {
                NamespaceDocItem => "./",
                TypeDocItem => "../",
                MemberDocItem => "../../",
                _ => string.Empty,
            } + basePath;

            return string.Empty;
        }

        private ImmutableDictionary<string, DocItem>.Builder FlattenDocItems(ImmutableDictionary<string, DocItem>.Builder createBuilder, IEnumerable<DocItem> items)
        {
            foreach (var item in items.OrEmpty())
            {
                createBuilder.Add(item.Id, item);
                FlattenDocItems(createBuilder, item.Items);
            }

            return createBuilder;
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
}
