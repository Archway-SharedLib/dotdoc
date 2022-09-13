using Microsoft.CodeAnalysis;

namespace DotDoc.Core;

/// <summary>
/// 利用する <see cref="SymbolDisplayFormat"/> の設定を定義します。
/// </summary>
public static class SymbolDisplayFormats
{
    /// <summary>
    /// メソッドの <c>DisplayName</c> プロパティのフォーマットを定義します。
    /// </summary>
    public static SymbolDisplayFormat MethodDisplayNameFormat { get; } = new (
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions:
        SymbolDisplayMemberOptions.IncludeParameters |
        // SymbolDisplayMemberOptions.IncludeContainingType |
        SymbolDisplayMemberOptions.IncludeExplicitInterface,
        parameterOptions:
        SymbolDisplayParameterOptions.IncludeParamsRefOut |
        SymbolDisplayParameterOptions.IncludeType,
        miscellaneousOptions:
        SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
        SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
        SymbolDisplayMiscellaneousOptions.UseAsterisksInMultiDimensionalArrays |
        SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName |
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
    
    /// <summary>
    /// タイプの <c>DisplayName</c> プロパティのフォーマットを定義します。
    /// </summary>
    public static SymbolDisplayFormat TypeDisplayNameFormat { get; } = new (
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions:
        SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
        SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
        SymbolDisplayMiscellaneousOptions.UseAsterisksInMultiDimensionalArrays |
        SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName |
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
}