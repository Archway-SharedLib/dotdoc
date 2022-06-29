// See https://aka.ms/new-console-template for more information

using DotDoc;
using DotDoc.Core;

const string InputFile = "../../../../../dotdoc.sample.sln";

var engine = new DotDocEngine(new ConsoleLogger());
var docItems = await engine.ExecuteAsync(new DotDocEngineOptions()
{
    InputFileName = InputFile,
    ExcludeIdPatterns = new string[] 
    {
        "N:SampleLib.Ns1.ExcludeDir"
    }
});

foreach(var aItem in docItems.OfType<AssemblyDocItem>())
{
    Console.WriteLine($"{aItem.Id} : {aItem.Name}");
    foreach(var nItem in aItem.Namespaces ?? Enumerable.Empty<NamespaceDocItem>())
    {
        Console.WriteLine($"  {nItem.Id} : {nItem.Name}");
        foreach (var tItem in nItem.Types ?? Enumerable.Empty<TypeDocItem>())
        {
            Console.WriteLine($"    {tItem.Id} : {tItem.Name}");

            foreach (var mItem in tItem.Members ?? Enumerable.Empty<MemberDocItem>())
            {
                Console.WriteLine($"      {mItem.Id} : {mItem.Name}");
            }
        }
    }
}

