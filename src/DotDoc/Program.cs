// See https://aka.ms/new-console-template for more information

using DotDoc;
using DotDoc.Core;
using DotDoc.Core.Read;

const string InputFile = "../../../../../dotdoc.sample.sln";

var engine = new DotDocEngine(new ConsoleLogger());
var options = new DotDocEngineOptions()
{
    InputFileName = InputFile,
    ExcludeIdPatterns = new string[]
    {
        "N:SampleLib.Ns1.ExcludeDir"
    },
    OutputDir = "Output"
};

var docItems = await engine.ReadAsync(options);

foreach(var aItem in docItems.OfType<AssemblyDocItem>())
{
    Console.WriteLine($"{aItem.Id} : {aItem.Name} [{aItem.XmlDocInfo?.Summary}]");
    foreach(var nItem in aItem.Namespaces ?? Enumerable.Empty<NamespaceDocItem>())
    {
        Console.WriteLine($"  {nItem.Id} : {nItem.Name} [{nItem.XmlDocInfo?.Summary}]");
        foreach (var tItem in nItem.Types ?? Enumerable.Empty<TypeDocItem>())
        {
            Console.WriteLine($"    {tItem.Id} : {tItem.Name} [{tItem.XmlDocInfo?.Summary}]");

            foreach (var mItem in tItem.Members ?? Enumerable.Empty<MemberDocItem>())
            {
                Console.WriteLine($"      {mItem.Id} : {mItem.Name} [{mItem.XmlDocInfo?.Summary}]");
            }
        }
    }
}

await engine.WriteAsync(docItems, options);