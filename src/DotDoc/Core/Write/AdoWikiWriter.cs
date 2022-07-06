using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core.Write
{
    internal class AdoWikiWriter : IWriter
    {
        private readonly DotDocEngineOptions _options;

        public AdoWikiWriter(DotDocEngineOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task WriteAsync(IEnumerable<DocItem> docItems)
        {
            foreach(var assemDocItem in docItems.OfType<AssemblyDocItem>())
            {
                foreach(var nsDocItem in assemDocItem.Namespaces.OrEmpty())
                {
                    await WriteNamespaceAsync(nsDocItem);
                }
            }
        }

        private async Task WriteNamespaceAsync(NamespaceDocItem nsDocItem)
        {
            var rootDir = new DirectoryInfo(_options.OutputDir);
            var safeName = SafeFileOrDirectoryName(nsDocItem.DisplayName);
            rootDir.CreateSubdirectory(safeName);

            var sb = new StringBuilder();
            sb.AppendLine($"# Namespace {nsDocItem.DisplayName}");
            sb.AppendLine();
            AppendTypeListMd<ClassDocItem>("Classes", nsDocItem, sb);
            sb.AppendLine();
            AppendTypeListMd<InterfaceDocItem>("Interfaces", nsDocItem, sb);
            sb.AppendLine();
            AppendTypeListMd<StructDocItem>("Structs", nsDocItem, sb);
            sb.AppendLine();
            AppendTypeListMd<EnumDocItem>("Enums", nsDocItem, sb);

            await File.WriteAllTextAsync(Path.Combine(rootDir.FullName, safeName + ".md"), sb.ToString(), Encoding.UTF8);
        }

        private void AppendTypeListMd<T>(string title, NamespaceDocItem nsDocItem, StringBuilder sb) where T : TypeDocItem
        {
            var docItems = nsDocItem.Types.OrEmpty().OfType<T>();
            if (!docItems.Any()) return;

            var safeName = SafeFileOrDirectoryName(nsDocItem.DisplayName);

            sb.AppendLine($"## {title}");
            sb.AppendLine();
            sb.AppendLine("| Name | Summary |");
            sb.AppendLine("|------|---------|");

            foreach (var typeDocItem in docItems)
            {
                var safeTypeName = SafeFileOrDirectoryName(typeDocItem.DisplayName);
                sb.AppendLine($"| [{SafeMdText(typeDocItem.DisplayName)}]({safeName}/{safeTypeName}.md) | {SafeMdText(typeDocItem.XmlDocInfo?.Summary)} |");
            }
        }

        private string SafeFileOrDirectoryName(string fileOrDirectoryName)
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

        private string SafeMdText(string text)
            => (text ?? string.Empty)
                .Replace("<", "\\<")
                .Replace(">", "\\>");

    }
}
