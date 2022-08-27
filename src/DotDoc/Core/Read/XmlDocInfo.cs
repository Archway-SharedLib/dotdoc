using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core.Read
{
    public class XmlDocInfo
    {
        public string? RawXml { get; set; }

        public string? Summary { get; set; }

        public string? Remarks { get; set; }

        public string? Example { get; set; }

        public string? Value { get; set; }

        public string? Returns { get; set; }

        public List<XmlDocNameTextInfo>? Parameters { get; set; }
        
        public List<XmlDocNameTextInfo>? TypeParameters { get; set; }
        
        public XmlDocInheritdocInfo? Inheritdoc { get; set; }
    }

    public class XmlDocNameTextInfo
    {
        public string? Name { get; set; }

        public string? Text { get; set; }
    }

    public class XmlDocInheritdocInfo
    {
        public string? Cref { get; set; }
    }
}
