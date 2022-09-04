using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DotDoc.Core.Read
{
    internal static class XmlDocParser
    {
        public static XmlDocInfo? Parse(ISymbol symbol, Compilation compilation)
        {
            var xmlText = symbol.GetDocumentationCommentXml();
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
                // RawXml = xmlText,
                Summary = GetNodeValue(nav, "/member/summary")?.Trim(),
                Remarks = GetNodeValue(nav, "/member/remarks")?.Trim(),
                Example = GetNodeValue(nav, "/member/example")?.Trim(),
                Value = GetNodeValue(nav, "/member/value")?.Trim(),
                Returns = GetNodeValue(nav, "/member/returns")?.Trim(),
                Parameters = GetNameTextInfo(nav, "/member/param"),
                TypeParameters = GetNameTextInfo(nav,"/member/typeparam"),
                // Inheritdoc = GetInheritdoc(nav)
            };
            var inheritdoc = GetInheritdoc(nav);

            if (inheritdoc is not null)
            {
                var baseSymbol = GetBaseDocSymbol(symbol, inheritdoc, compilation);
                if (baseSymbol is null)
                {
                    return result;
                }
                var baseXmlDocInfo = Parse(baseSymbol, compilation);
                if (baseXmlDocInfo is not null)
                {
                    result.Summary ??= baseXmlDocInfo.Summary;
                    result.Remarks ??= baseXmlDocInfo.Remarks;
                    result.Example ??= baseXmlDocInfo.Example;
                    result.Value ??= baseXmlDocInfo.Value;
                    result.Returns ??= baseXmlDocInfo.Returns;
                    result.Parameters = result.Parameters.Any() ? result.Parameters : baseXmlDocInfo.Parameters;
                    result.TypeParameters = result.TypeParameters.Any() ? result.TypeParameters : baseXmlDocInfo.TypeParameters;
                }
            }
            
            return result;
        }

        private static ISymbol? GetBaseDocSymbol(ISymbol source, XmlDocInheritdocInfo inheritInfo, Compilation compilation)
        {
            if (!string.IsNullOrWhiteSpace(inheritInfo.Cref))
            {
                var inheritSource = DocumentationCommentId.GetFirstSymbolForDeclarationId(inheritInfo.Cref, compilation);
                if (inheritSource is null)
                {
                    return null;
                }
                return inheritSource;
            }

            

            if (source is INamedTypeSymbol namedTypeSymbol)
            {
                var hasExplicitBaseType = HasExplicitBaseType(namedTypeSymbol);
                if (hasExplicitBaseType)
                {
                    return namedTypeSymbol.BaseType;
                }
                if (namedTypeSymbol.Interfaces.Length == 1)
                {
                    return namedTypeSymbol.Interfaces.First();
                }

                return null;
            }

            if (source is IMethodSymbol methodSymbol) return methodSymbol.OverriddenMethod ?? ExplicitOrImplicitInterfaceImplementations(methodSymbol).FirstOrDefault();
            if (source is IPropertySymbol propertySymbol) return propertySymbol.OverriddenProperty ?? ExplicitOrImplicitInterfaceImplementations(propertySymbol).FirstOrDefault();
            if (source is IEventSymbol eventSymbol) return eventSymbol.OverriddenEvent ?? ExplicitOrImplicitInterfaceImplementations(eventSymbol).FirstOrDefault();

            return null;
        }
        
        public static ImmutableArray<ISymbol> ExplicitOrImplicitInterfaceImplementations(ISymbol symbol)
        {
            if (symbol.Kind is not SymbolKind.Method and not SymbolKind.Property and not SymbolKind.Event)
                return ImmutableArray<ISymbol>.Empty;

            var containingType = symbol.ContainingType;
            var query = from iface in containingType.AllInterfaces
                from interfaceMember in iface.GetMembers()
                let impl = containingType.FindImplementationForInterfaceMember(interfaceMember)
                where symbol.Equals(impl)
                select interfaceMember;
            return query.ToImmutableArray();
        }

        private static bool HasExplicitBaseType(INamedTypeSymbol symbol)
        {
            if (symbol.BaseType is null) return false;
            var baseType = symbol.BaseType.GetDocumentationCommentId();
            return baseType is not ("T:System.Object" or "T:System.ValueType" or "T:System.Enum");
        }

        private static XmlDocInheritdocInfo? GetInheritdoc(XPathNavigator nav)
        {
            var node = nav.SelectSingleNode("/member/inheritdoc");
            if (node is null) return null;
            return new XmlDocInheritdocInfo()
            {
                Cref = node.GetAttribute("cref", string.Empty)
            };
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
