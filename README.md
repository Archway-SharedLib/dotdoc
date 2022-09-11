# dotdoc

Reads XML document comments contained in the solution or project file and generates Markdown. It can be viewed on the Azure DevOps Wiki.

## Usage

TODO: I'll write when I release it.

## Commands

### init

```
dotdoc init
```

Generate a `.dotdoc` file to the current directory containing the default settings. If there is a solution or project file in the current directory, it will automatically be set to the `InputFile` option.

### run

```
dotdoc run
```

Reads the `.dotdoc` file in the current directory and generates a Markdown file.

## Configuration

Configurations are set in the `.dotdoc` file.

```json
{
  "InputFileName": "src/sample.sln",
  "ExcludeIdPatterns": [
    "*:ExcludeNamespace.*",
    "*:ExcludeNamespace",
    "A:*.Tests"
  ],
  "Accessibility": [
    "Protected",
    "Public",
    "ProtectedInternal"
  ],
  "OutputDir": "./apidocs",
  "RemoveOutputDir": true,
  "IgnoreEmptyNamespace": true,
  "ExcludeDocumentClass": true,
  "LogLevel": "Info"
}
```

### InputFileName

Specify the name of the solution or project file to be read.

### ExcludeIdPatterns

Specify the pattern of ID to exclude. The asterisk(`*`) matches any string. The format of the ID conforms to [standard XML document comments](https://docs.microsoft.com/dotnet/csharp/language-reference/xmldoc/#id-strings).

The tool uses custom member identifiers.

| Character | Member type | Notes |
|-----------|-------------|-------|
| A | assembly |  |

### Accesibility

Specify the accesibility to be output. 

- `Public` : Generates documentation for 'public' access modifier.
- `Protected` : Generates documentation for 'protected' access modifier.
- `Internal` : Generates documentation for 'internal' access modifier.
- `Private` : Generates documentation for 'private' access modifier.
- `ProtectedInternal` : Generates documentation for 'protected internal' access modifier.
- `PrivateProtected` : Generates documentation for 'private protected' access modifier.

### OutputDir

Specifies the destination directory. If it does not exist, it will be created.

### RemoveOutputDir

Specifies whether the output directory is deleted before processing.

### IgnoreEmptyNamespace

Specifies whether or not to output namespaces that do not contain types.

### ExcliceDocumentClass

Specify whether to exclude the `AssemblyDoc` and `NamespaceDoc` classes.

## Documentation Xml Elements

[Standard elements](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments
) can be used to describe the document. However, in order to structure the document, the types of elements are classified.

### Section Elements

Section elements may only be used for the root.

- [\<summary\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#summary)
- [\<remarks\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#remarks)
- [\<example\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#example)
- [\<param\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#param)
- [\<typeparam\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#typeparam)
- [\<returns\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#returns)
- [\<value\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#value)

### Block Elements

Block elements are available as child nodes of section elements. Elements other than the [\<para\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#para) element cannot be nested. 

- [\<para\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#para)
- [\<list\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#list): If the Type attribute is not specified, it is not displayed.
- [\<code\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#code)

### Inline Elements

Inline elements are available as childe nodes of section and block elements. Line breaks will be removed.

- [\<c\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#c)
- [\<see\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#see)
- [\paramref\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#paramref)
- [\<typeparamref\>](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments#typeparamref)

### Miscellaneous Elements

Several special elements are available.

#### `<inheritdoc>` element

This element can help minimize the effort required to document complex APIs by allowing common documentation to be inherited from base types/members.

```xml
<inheritdoc [cref="member"] />
```

If this element is specified, it is inherited on a section element basis.

#### `<overloads>` element

This element is used to define the content that should appear on the auto-generated overloads topic for a given set of member overloads.

```xml
<overloads>summary comment</overloads>

<!--or-->

<overloads>
    <summary>summary comment</summary>
    <remarks>remarks comment</remarks>
    <!--each section elements-->
</overloads>
```