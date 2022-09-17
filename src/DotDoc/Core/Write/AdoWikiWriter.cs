using System.Collections.Immutable;
using System.Text;
using DotDoc.Core.Models;
using DotDoc.Core.Read;
using DotDoc.Core.Write.Page;

namespace DotDoc.Core.Write
{
    public class AdoWikiWriter : IWriter, IFileSystemOperation
    {
        private readonly List<IDocItem> _docItems;
        private readonly DotDocEngineOptions _options;
        private readonly IFsModel _fsModel;
        private readonly ILogger _logger;
        private readonly TextTransform _textTransform;
        private readonly DocItemContainer _docItemContainer;

        public AdoWikiWriter(IEnumerable<IDocItem> docItems, DotDocEngineOptions options, IFsModel fsModel, ILogger logger)
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
            _logger.Info($"Write assembly: {assemDocItem.DisplayName}");
            
            var pageMd = new AssemblyPage(assemDocItem, _textTransform, _docItemContainer, _options).Write();
            
            var safeName = assemDocItem.ToFileName();
            var file = _fsModel.CreateFileModel(_fsModel.PathJoin(rootDir.GetFullName(), safeName + ".md"));
            file.WriteText(pageMd);

            var assemDir = _fsModel.CreateDirectoryModel(_fsModel.PathJoin(rootDir.GetFullName(), safeName));
            foreach (var nsDocItem in assemDocItem.Namespaces.OrEmpty())
            {
                await WriteNamespaceAsync(assemDir, nsDocItem);
            }
        }

        private async Task WriteNamespaceAsync(IDirectoryModel assemDir, NamespaceDocItem nsDocItem)
        {
            assemDir.CreateIfNotExists();

            if (_options.IgnoreEmptyNamespace && !nsDocItem.Types.Any())
            {
                return;
            }
            
            _logger.Trace($"Write namespace: {nsDocItem.DisplayName}");
            
            var pageMd = new NamespacePage(nsDocItem, _textTransform, _docItemContainer).Write();
            
            var safeName = SafeFileOrDirectoryName(nsDocItem.ToFileName());
            var file = _fsModel.CreateFileModel(_fsModel.PathJoin(assemDir.GetFullName(), safeName + ".md"));
            file.WriteText(pageMd);
            
            var nsDir = _fsModel.CreateDirectoryModel(_fsModel.PathJoin(assemDir.GetFullName(), safeName));
            foreach (var typeDocItem in nsDocItem.Types.OrEmpty())
            {
                await WriteTypeAsync(nsDir, typeDocItem);
            }
        }

        private async Task WriteTypeAsync(IDirectoryModel nsDir, TypeDocItem typeDocItem)
        {
            nsDir.CreateIfNotExists();

            IPage? page = typeDocItem switch
            {
                ClassDocItem i => new ClassPage(i, _textTransform, _docItemContainer),
                InterfaceDocItem i => new InterfacePage(i, _textTransform, _docItemContainer),
                EnumDocItem i => new EnumPage(i, _textTransform, _docItemContainer),
                StructDocItem i => new StructPage(i, _textTransform, _docItemContainer),
                DelegateDocItem i => new DelegatePage(i, _textTransform, _docItemContainer),
                _ => null
            };
            if (page is null) return;
            
            _logger.Trace($"Write type: {typeDocItem.DisplayName}");
            
            var typeDirOrFile = _fsModel.PathJoin(nsDir.GetFullName(), SafeFileOrDirectoryName(typeDocItem.ToFileName()));
            _fsModel.CreateFileModel(typeDirOrFile + ".md").WriteText(page.Write());
            
            if (typeDocItem is EnumDocItem) return;
            
            var typeDir = _fsModel.CreateDirectoryModel(typeDirOrFile);

            foreach (var memberDocItem in PrepareMemberDocItems(typeDocItem.Members.OrEmpty()))
            {
                await WriteMemberAsync(typeDir, typeDocItem, memberDocItem);
            }
        }

        private IEnumerable<IMemberDocItem> PrepareMemberDocItems(IEnumerable<IMemberDocItem> members)
        {
            var nonMethodDocItems = new List<IMemberDocItem>();
            var methodDocItems = new Dictionary<string, List<MethodDocItem>>();
            var ctorDocItems = new List<ConstructorDocItem>();
            
            foreach (var member in members)
            {
                if (member is MethodDocItem method)
                {
                    var name = method.Name!;
                    var overload = methodDocItems.ContainsKey(name) ? methodDocItems[name] : new();
                    overload.Add(method);
                    methodDocItems[name] = overload;
                }
                else if (member is ConstructorDocItem ctor)
                {
                    ctorDocItems.Add(ctor);
                }
                else
                {
                    nonMethodDocItems.Add(member);
                }
            }

            if (ctorDocItems.Any())
            {
                nonMethodDocItems.Add(new OverloadConstructorDocItem(ctorDocItems));
            }
            
            foreach (var method in methodDocItems.Values)
            {
                if (method.Count == 1)
                {
                    nonMethodDocItems.Add(method.First());
                }
                else
                {
                    nonMethodDocItems.Add(new OverloadMethodDocItem(method));
                }
            }

            return nonMethodDocItems;
        }

        private async Task WriteMemberAsync(IDirectoryModel typeDir, TypeDocItem typeDocItem, IMemberDocItem memberDocItem)
        {
            var sb = memberDocItem switch
            {
                OverloadConstructorDocItem conDocItem => new StringBuilder(new ConstructorPage(conDocItem, _textTransform, _docItemContainer).Write()),
                MethodDocItem methodDocItem => new StringBuilder(new MethodPage(methodDocItem, _textTransform, _docItemContainer).Write()),
                PropertyDocItem propDocItm => new StringBuilder(new PropertyPage(propDocItm, _textTransform, _docItemContainer).Write()),
                FieldDocItem fieldDocItm => new StringBuilder(new FieldPage(fieldDocItm, _textTransform, _docItemContainer).Write()),
                OverloadMethodDocItem oMethodDocItem => new StringBuilder(new OverloadMethodPage(oMethodDocItem, _textTransform, _docItemContainer).Write()),
                _ => null
            };
            if (sb is null) return;

            typeDir.CreateIfNotExists();
            
            _logger.Trace($"Write member: {memberDocItem.DisplayName}");

            _fsModel.CreateFileModel(_fsModel.PathJoin(typeDir.GetFullName(), SafeFileOrDirectoryName(memberDocItem.ToFileName())) + ".md").WriteText(sb.ToString());
        }
        
        public string GetRelativeLink(IDocItem source, IDocItem dest)
        {
            string FileName(string target) => SafeFileOrDirectoryName(_textTransform.EscapeMdText(target));
            
            var basePath = dest switch
            {
                AssemblyDocItem => $"{FileName(dest.ToFileName())}.md",
                NamespaceDocItem n => $"{FileName(_docItemContainer.Get(n.AssemblyId).ToFileName())}/{FileName(dest.ToFileName())}.md",
                TypeDocItem t => $"{FileName(_docItemContainer.Get(t.AssemblyId).ToFileName())}/{FileName(_docItemContainer.Get(t.NamespaceId).ToFileName())}/{FileName(dest.ToFileName())}.md",
                IMemberDocItem m => $"{FileName(_docItemContainer.Get(m.AssemblyId).ToFileName())}/{FileName(_docItemContainer.Get(m.NamespaceId).ToFileName())}/{FileName(_docItemContainer.Get(m.TypeId).ToFileName())}/{FileName(dest.ToFileName())}.md",
                _ => string.Empty,
            };
            return source switch
            {
                AssemblyDocItem => "./",
                NamespaceDocItem => "../",
                TypeDocItem => "../../",
                IMemberDocItem => "../../../",
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
    }
}
