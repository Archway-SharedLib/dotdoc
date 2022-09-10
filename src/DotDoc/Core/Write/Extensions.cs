using DotDoc.Core.Models;

namespace DotDoc.Core.Write;

public static class Extensions
{
    public static string? ToFileName(this IDocItem source)
    {
        return SafeFileOrDirectoryName(source switch
        {
            OverloadConstructorDocItem oc => "$ctor",
            MethodDocItem m => m.Name,
            ConstructorDocItem c => "$ctor",
            TypeDocItem t => t.MetadataName,
            _ => source.DisplayName
        });
    }
    
    public static string SafeFileOrDirectoryName(string? fileOrDirectoryName)
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
    
    
    public static TypeInfo GetLinkTypeInfo(this TypeInfo info)
    {
        while (info.LinkType is not null)
        {
            info = info.LinkType;
        }
        return info;
    }
}