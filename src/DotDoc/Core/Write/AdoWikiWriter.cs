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
                    await WriteNamespaceAsync(rootDir, assemDocItem, nsDocItem);
                }
            }
        }

        private async Task WriteNamespaceAsync(DirectoryInfo rootDir, AssemblyDocItem assemDocItem, NamespaceDocItem nsDocItem)
        {
            var safeName = SafeFileOrDirectoryName(nsDocItem.DisplayName);
            var nsDir = new DirectoryInfo(Path.Join(rootDir.FullName, safeName));

            var sb = new StringBuilder();
            AppendTitle(sb, "Namespace", nsDocItem.DisplayName);

            sb.AppendLine($"assembly: {_textTransform.EscapeMdText(assemDocItem.DisplayName)}").AppendLine();

            AppendTypeListMd<ClassDocItem>("Classes", nsDocItem, sb);
            sb.AppendLine();
            AppendTypeListMd<StructDocItem>("Structs", nsDocItem, sb);
            sb.AppendLine();
            AppendTypeListMd<InterfaceDocItem>("Interfaces", nsDocItem, sb);
            sb.AppendLine();
            AppendTypeListMd<EnumDocItem>("Enums", nsDocItem, sb);
            sb.AppendLine();
            AppendTypeListMd<DelegateDocItem>("Delegates", nsDocItem, sb);


            await File.WriteAllTextAsync(Path.Combine(rootDir.FullName, safeName + ".md"), sb.ToString(), Encoding.UTF8);

            foreach (var typeDocItem in nsDocItem.Types.OrEmpty())
            {
                await WriteTypeAsync(nsDir, nsDocItem, typeDocItem);
            }
        }

        private async Task WriteTypeAsync(DirectoryInfo nsDir, NamespaceDocItem nsDocItem, TypeDocItem typeDocItem)
        {
            if (!nsDir.Exists) nsDir.Create();

            var sb = new StringBuilder();
            AppendTitle(sb, GetTypeTypeName(typeDocItem), typeDocItem.DisplayName);

            sb.AppendLine($"namespace: [{_textTransform.EscapeMdText(nsDocItem.DisplayName)}]({GetRelativeLink(typeDocItem, nsDocItem)})<br />");
            sb.AppendLine($"assembly: {_textTransform.EscapeMdText(_flatItems[typeDocItem.AssemblyId]?.DisplayName)}").AppendLine();

            sb.AppendLine(_textTransform.ToMdText(typeDocItem, typeDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();

            AppendMemberListMd<ConstructorDocItem>("Constructors", typeDocItem, sb);
            sb.AppendLine();
            AppendMemberListMd<MethodDocItem>("Methods", typeDocItem, sb);
            sb.AppendLine();
            AppendMemberListMd<PropertyDocItem>("Properties", typeDocItem, sb);
            sb.AppendLine();
            AppendMemberListMd<FieldDocItem>("Fields", typeDocItem, sb);
            sb.AppendLine();
            AppendMemberListMd<EventDocItem>("Events", typeDocItem, sb);

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
                ConstructorDocItem conDocItem => CreteConstructorPageText(typeDocItem, conDocItem),
                _ => null
            };
            if (sb is null) return;

            if (!typeDir.Exists) typeDir.Create();
            await File.WriteAllTextAsync(Path.Combine(typeDir.FullName, SafeFileOrDirectoryName(memberDocItem.DisplayName)) + ".md", sb.ToString(), Encoding.UTF8);

        }

        private StringBuilder CreteConstructorPageText(TypeDocItem typeDocItem, ConstructorDocItem memberDocItem)
        {
            var sb = new StringBuilder();
            AppendTitle(sb, "Constructor", memberDocItem.DisplayName);
            // AppendNamespaceAssemblyInformation(sb, memberDocItem.Id, memberDocItem.NamespaceId, memberDocItem.AssemblyId);
            var nsDocItem = _flatItems[memberDocItem.NamespaceId];
            sb.AppendLine($"namespace: [{_textTransform.EscapeMdText(nsDocItem?.DisplayName)}]({GetRelativeLink(memberDocItem, nsDocItem)})<br />");
            sb.AppendLine($"assembly: {_textTransform.EscapeMdText(_flatItems[memberDocItem.AssemblyId]?.DisplayName)}").AppendLine();

            sb.AppendLine(_textTransform.ToMdText(memberDocItem, memberDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();

            var parameters = memberDocItem.Parameters.OrEmpty();
            if(parameters.Any())
            {
                sb.AppendLine($"## Parameters").AppendLine();
                sb.AppendLine("| Type | Name | Summary |");
                sb.AppendLine("|------|------|---------|");
                foreach(var param in parameters)
                {
                    sb.AppendLine($@"| {_textTransform.EscapeMdText(param.TypeDisplayName)} | {_textTransform.EscapeMdText(param.DisplayName)} | {_textTransform.ToMdText(memberDocItem, param, t => t.XmlDocText, true)} |");
                }
            }

            return sb;
        }

        private void AppendTitle(StringBuilder sb, string type, string title) =>
            sb.AppendLine($"# {title} {type}").AppendLine();
        
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

        private string GetTypeTypeName(TypeDocItem docItem) =>
            docItem is ClassDocItem ? "Class" :
            docItem is InterfaceDocItem ? "Interface" :
            docItem is EnumDocItem ? "Enum" :
            docItem is StructDocItem ? "Struct" : string.Empty;

        private void AppendTypeListMd<T>(string title, NamespaceDocItem nsDocItem, StringBuilder sb) where T : TypeDocItem
        {
            var docItems = nsDocItem.Types.OrEmpty().OfType<T>();
            if (!docItems.Any()) return;

            sb.AppendLine($"## {title}");
            sb.AppendLine();
            sb.AppendLine("| Name | Summary |");
            sb.AppendLine("|------|---------|");

            foreach (var typeDocItem in docItems)
            {
                sb.AppendLine($@"| [{_textTransform.EscapeMdText(typeDocItem.DisplayName)}]({GetRelativeLink(nsDocItem, typeDocItem)}) | {_textTransform.ToMdText(nsDocItem, typeDocItem, t => t.XmlDocInfo?.Summary, true)} |");
            }
        }

        private void AppendMemberListMd<T>(string title, TypeDocItem typeDocItem, StringBuilder sb) where T : MemberDocItem
        {
            var docItems = typeDocItem.Members.OrEmpty().OfType<T>();
            if (!docItems.Any()) return;

            sb.AppendLine($"## {title}");
            sb.AppendLine();
            sb.AppendLine("| Name | Summary |");
            sb.AppendLine("|------|---------|");

            var isEnumField = docItems.First() is FieldDocItem && typeDocItem is EnumDocItem;

            foreach (var memberDocItem in docItems)
            {
                if (isEnumField)
                {
                    sb.AppendLine($@"| {_textTransform.EscapeMdText(memberDocItem.DisplayName)} | {_textTransform.ToMdText(typeDocItem, memberDocItem, t => t.XmlDocInfo?.Summary ?? string.Empty, true)} |");
                }
                else
                {
                    sb.AppendLine($@"| [{_textTransform.EscapeMdText(memberDocItem.DisplayName)}]({GetRelativeLink(typeDocItem, memberDocItem)}) | {_textTransform.ToMdText(typeDocItem, memberDocItem, t => t.XmlDocInfo?.Summary ?? string.Empty, true)} |");
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

    }
}
