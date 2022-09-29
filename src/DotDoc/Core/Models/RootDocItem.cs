using DotDoc.Core.Read;
using DotDoc.Core.Write;

namespace DotDoc.Core.Models
{
    public class RootDocItem : IDocItem
    {
        private readonly AssembliesPageOptions _options;

        public RootDocItem(AssembliesPageOptions? options, IFsModel fsModel, IEnumerable<AssemblyDocItem> assemblies)
        {
            _options = options ?? new AssembliesPageOptions();
            XmlDocInfo = LoadXmlDocumentInfo(fsModel);
            DisplayName = _options.Name;
            Items = assemblies;
            if(!string.IsNullOrEmpty(_options.Summary))
            {
                XmlDocInfo.Summary = _options.Summary;
            }
        }

        private XmlDocInfo LoadXmlDocumentInfo(IFsModel fsModel)
        {
            if (string.IsNullOrWhiteSpace(_options.XmlDocumentFile)) return new XmlDocInfo();
            var fileModel = fsModel.CreateFileModel(_options.XmlDocumentFile);
            if(fileModel is null || !fileModel.Exists()) return new XmlDocInfo();
            var xmlText = fileModel.ReadAll();
            return XmlDocParser.ParseString(xmlText) ?? new XmlDocInfo();
        }

        public string? Id => string.Empty;

        public string? DisplayName { get; }

        public IEnumerable<IDocItem>? Items { get; }

        public XmlDocInfo? XmlDocInfo { get; }

        public string ToDeclareCSharpCode() => string.Empty;
    }
}
