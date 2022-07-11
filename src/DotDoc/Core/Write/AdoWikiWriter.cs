using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotDoc.Core.Write
{
    internal class AdoWikiWriter : IWriter, IFileSystemOperation
    {
        private readonly List<DocItem> _docItems;
        private readonly DotDocEngineOptions _options;
        private readonly TextTransform _textTransform;
        private readonly ImmutableDictionary<string,DocItem> _flatItems;

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
            foreach(var assemDocItem in _docItems.OfType<AssemblyDocItem>())
            {
                foreach(var nsDocItem in assemDocItem.Namespaces.OrEmpty())
                {
                    await WriteNamespaceAsync(rootDir, assemDocItem, nsDocItem);
                }
            }
        }

        private async Task WriteNamespaceAsync(DirectoryInfo rootDir, AssemblyDocItem assemDocItem, NamespaceDocItem nsDocItem)
        {
            var safeName = SafeFileOrDirectoryName(nsDocItem.DisplayName);
            var nsDir = rootDir.CreateSubdirectory(safeName);

            var sb = new StringBuilder();
            sb.AppendLine($"# {nsDocItem.DisplayName} Namespace").AppendLine();
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
            
            foreach (var typeDocItem in nsDocItem.Types)
            {
                await WriteTypeAsync(nsDir, nsDocItem, typeDocItem);
            }
        }

        private async Task WriteTypeAsync(DirectoryInfo nsDir, NamespaceDocItem nsDocItem, TypeDocItem typeDocItem)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {typeDocItem.DisplayName} {GetTypeTypeName(typeDocItem)}").AppendLine();

            sb.AppendLine($"namespace: [{_textTransform.EscapeMdText(nsDocItem.DisplayName)}]({GetRelativeLink(typeDocItem, nsDocItem)})<br />");
            sb.AppendLine($"assembly: {_textTransform.EscapeMdText(_flatItems[typeDocItem.AssemblyId]?.DisplayName)}").AppendLine();

            sb.AppendLine(_textTransform.ToMdText(typeDocItem, typeDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            
            AppendMemberListMd<MethodDocItem>("Methods", typeDocItem, sb);
            sb.AppendLine();
            AppendMemberListMd<PropertyDocItem>("Properties", typeDocItem, sb);
            sb.AppendLine();
            AppendMemberListMd<FieldDocItem>("Fields", typeDocItem, sb);
            sb.AppendLine();
            AppendMemberListMd<EventDocItem>("Events", typeDocItem, sb);
            
            await File.WriteAllTextAsync(Path.Combine(nsDir.FullName, SafeFileOrDirectoryName(typeDocItem.DisplayName) + ".md"), sb.ToString(), Encoding.UTF8);
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
                sb.AppendLine($@"| [{
                    _textTransform.EscapeMdText(typeDocItem.DisplayName)
                }]({GetRelativeLink(nsDocItem, typeDocItem)}) | {
                    _textTransform.ToMdText(nsDocItem, typeDocItem, t => t.XmlDocInfo?.Summary, true)
                } |");
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

            foreach (var memberDocItem in docItems)
            {
                sb.AppendLine($@"| [{
                    _textTransform.EscapeMdText(memberDocItem.DisplayName)
                }]({GetRelativeLink(typeDocItem, memberDocItem)}) | {
                    _textTransform.ToMdText(typeDocItem, memberDocItem, t => t.XmlDocInfo?.Summary ?? string.Empty, true)
                } |");
            }
        }

        public string GetRelativeLink(DocItem source, DocItem dest)
        {
            if (source is NamespaceDocItem)
            {
                if (dest is NamespaceDocItem)
                {
                    return $"./{SafeFileOrDirectoryName(dest.DisplayName)}.md";
                }
                if (dest is TypeDocItem typeDestDocItem)
                {
                    var destNamespace = _flatItems[typeDestDocItem.NamespaceId];
                    return $"./{SafeFileOrDirectoryName(destNamespace.DisplayName)}/{SafeFileOrDirectoryName(dest.DisplayName)}.md";
                }
            }
            if (source is TypeDocItem)
            {
                if (dest is NamespaceDocItem)
                {
                    return $"../{SafeFileOrDirectoryName(dest.DisplayName)}.md";
                }
                if (dest is TypeDocItem typeDestDocItem)
                {
                    var destNamespace = _flatItems[typeDestDocItem.NamespaceId];
                    return $"../{SafeFileOrDirectoryName(destNamespace.DisplayName)}/{SafeFileOrDirectoryName(dest.DisplayName)}.md";
                }
            }

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
