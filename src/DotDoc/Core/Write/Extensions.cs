namespace DotDoc.Core.Write;

public static class Extensions
{
    public static string? ToFileName(this DocItem source)
    {
        return source switch
        {
            OverloadConstructorDocItem oc => "$ctor",
            MethodDocItem m => m.Name,
            ConstructorDocItem c => "$ctor",
            TypeDocItem t => t.MetadataName,
            _ => source.DisplayName
        };
    }
}