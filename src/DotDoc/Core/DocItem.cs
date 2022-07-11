using DotDoc.Core.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core
{
    public abstract class DocItem
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public XmlDocInfo? XmlDocInfo { get; set; }

        public virtual IEnumerable<DocItem>? Items { get; } = Enumerable.Empty<DocItem>();

    }

    public class AssemblyDocItem : DocItem
    {
        public List<NamespaceDocItem>? Namespaces { get; set; }

        public override IEnumerable<DocItem>? Items => Namespaces;
    }

    public class NamespaceDocItem : DocItem
    {
        public List<TypeDocItem>? Types { get; set; }

        public string? AssemblyId { get; set; }
        
        public override IEnumerable<DocItem>? Items => Types;
    }

    public class TypeDocItem : DocItem
    {
        public List<MemberDocItem>? Members { get; set; }

        public string? AssemblyId { get; set; }
        
        public string? NamespaceId { get; set; }
        
        public override IEnumerable<DocItem>? Items => Members;
    }

    public class ClassDocItem : TypeDocItem
    {
    }

    public class InterfaceDocItem : TypeDocItem
    {
    }

    public class EnumDocItem : TypeDocItem
    {
    }

    public class StructDocItem : TypeDocItem
    {
    }

    public abstract class MemberDocItem : DocItem
    {
        public string? TypeId { get; set; }
    }

    public class FieldDocItem : MemberDocItem
    {
    }

    public class PropertyDocItem : MemberDocItem
    {
    }

    public class MethodDocItem : MemberDocItem
    {
    }

    public class EventDocItem : MemberDocItem
    {
    }
}
