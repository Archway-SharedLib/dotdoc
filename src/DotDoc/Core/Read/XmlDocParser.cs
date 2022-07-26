using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DotDoc.Core.Read
{
    internal static class XmlDocParser
    {
        public static XmlDocInfo? Parse(string? xmlText)
        {
            if (string.IsNullOrEmpty(xmlText)) return null;
            if (xmlText.StartsWith("<!-- Badly formed XML comment ignored for member")) return null;
            XDocument xdoc;
            try
            {
                xdoc = XDocument.Parse(xmlText);
            }
            catch
            {
                return null;
            }
            var nav = xdoc.CreateNavigator();

            var result = new XmlDocInfo()
            {
                RawXml = xmlText,
                Summary = GetNodeValue(nav, "/member/summary")?.Trim(),
                Remarks = GetNodeValue(nav, "/member/remarks")?.Trim(),
                Example = GetNodeValue(nav, "/member/example")?.Trim(),
                Value = GetNodeValue(nav, "/member/value")?.Trim(),
                Returns = GetNodeValue(nav, "/member/returns")?.Trim(),
                Parameters = GetNameTextInfo(nav, "/member/param"),
                TypeParameters = GetNameTextInfo(nav,"/member/typeparam"),
            };

            return result;
        }

        private static List<XmlDocNameTextInfo> GetNameTextInfo(XPathNavigator nav, string path)
        {
            var result = new List<XmlDocNameTextInfo>();
            var nodeIterator = nav.Select(path);
            foreach (XPathNavigator nodeNav in nodeIterator)
            {
                result.Add(new()
                {
                    Text = GetInnerXml(nodeNav),
                    Name = nodeNav.GetAttribute("name", string.Empty)
                });
                //    var text = GetInnerXml(nodeNav);
                //    var nodeNav.GetAttribute("name", string.Empty)
                //}
            }
            return result;
        }

        private static string? GetNodeValue(XPathNavigator nav, string path)
        {
            var nodeNav = nav.SelectSingleNode(path);
            if (nodeNav is null) return null;
            return GetInnerXml(nodeNav);
        }

        private static string GetInnerXml(XPathNavigator node)
        {
            using var sw = new StringWriter(CultureInfo.InvariantCulture);
            using (var tw = new XmlTextWriter(sw))
            {
                if (node.MoveToFirstChild())
                {
                    do
                    {
                        tw.WriteNode(node, true);
                    } while (node.MoveToNext());
                    node.MoveToParent();
                }
            }

            return sw.ToString();
        }
    }
}
