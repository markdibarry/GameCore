using System;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Items;

public abstract class AItemDB
{
    protected AItemDB()
    {
        _items = BuildDB();
    }

    private readonly AItem[] _items;
    public IReadOnlyCollection<AItem> Items => _items;

    public AItem? GetItem(string id)
    {
        return Array.Find(_items, item => item.Id.Equals(id));
    }

    public IEnumerable<AItem> GetItemsByCategory(string itemCategoryId)
    {
        return _items.Where(item => item.ItemCategory.Id.Equals(itemCategoryId));
    }

    protected abstract AItem[] BuildDB();
}
