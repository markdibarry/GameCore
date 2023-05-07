using System;
using System.Collections.Generic;

namespace GameCore.Items;

public abstract class AItemCategoryDB
{
    protected AItemCategoryDB()
    {
        _categories = BuildDB();
    }

    private readonly ItemCategory[] _categories;
    public IReadOnlyCollection<ItemCategory> Categories => _categories;

    public ItemCategory? GetCategory(string id)
    {
        return Array.Find(_categories, category => category.Id.Equals(id));
    }

    protected abstract ItemCategory[] BuildDB();
}
