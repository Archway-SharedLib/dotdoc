using System.Diagnostics.CodeAnalysis;

namespace DotDoc.Core.Write;

public class DocItemContainer
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, Dictionary<string, DocItem>> _pool = new();
    private readonly Dictionary<string, DocItem> _cache = new();

    public DocItemContainer(IEnumerable<DocItem> docItems, ILogger logger)
    {
        _logger = logger;
        foreach (var item in docItems.OrEmpty())
        {
            if (item is AssemblyDocItem assemDocItem)
            {
                if(string.IsNullOrWhiteSpace(assemDocItem.Id)) continue;
                
                var poolByAssem = new Dictionary<string, DocItem>();
                _pool.Add(assemDocItem.Id, poolByAssem);
                poolByAssem.Add(assemDocItem.Id, assemDocItem);
                FlattenAssemblyItems(assemDocItem, poolByAssem);        
            }
        }
    }

    private void FlattenAssemblyItems(DocItem parentItem, Dictionary<string, DocItem> pool)
    {
        foreach (var item in parentItem.Items.OrEmpty())
        {
            if (pool.ContainsKey(item.Id))
            {
                _logger?.Trace($"すでに登録されています。{item.Id}");
                continue;
            }
            pool.Add(item.Id, item);
            FlattenAssemblyItems(item, pool);
        }
    }

    public DocItem? Get(string id)
    {
        if (_cache.ContainsKey(id)) return _cache[id];
        foreach (var value in _pool.Values)
        {
            if (value.ContainsKey(id))
            {
                var result = value[id];
                _cache[id] = result;
                return result;
            }
        }
        return null;
    }
    
    public DocItem? Get(string assemblyId, string id)
    {
        if (!_pool.ContainsKey(assemblyId)) return null;
        var value = _pool[assemblyId];
        if (!value.ContainsKey(id)) return null;
        return value[id];
    }
    
    public bool TryGet(string id, [NotNullWhen(true)] out DocItem? item)
    {
        item = Get(id);
        return item is not null;
    }

    public bool TryGet(string assemblyId, string id, [NotNullWhen(true)] out DocItem? item)
    {
        item = Get(assemblyId, id);
        return item is not null;
    }

}