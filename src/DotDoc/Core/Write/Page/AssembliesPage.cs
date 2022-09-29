using DotDoc.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core.Write.Page
{
    public class AssembliesPage : BasePage, IPage
    {
        private readonly RootDocItem _docItem;
        private readonly TextTransform _transform;

        public AssembliesPage(RootDocItem docItem, TextTransform transform, DocItemContainer itemContainer) : base(docItem, transform, itemContainer)
        {
            _docItem = docItem;
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        public string Write()
        {
            var sb = new TextBuilder();

            AppendTitle(sb, $"{_docItem.DisplayName}");

            sb.AppendLine(_transform.ToMdText(_docItem, _docItem, t => t.XmlDocInfo?.Summary)).AppendLine();

            AppendItemList<AssemblyDocItem>(sb, "Assemblies", _docItem.Items!);

            return sb.ToString();
        }
    }
}
