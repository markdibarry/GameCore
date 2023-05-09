using System.Collections.Generic;

namespace GameCore.Items;

public interface IItemCategoryDB
{
    ItemCategory? GetCategory(string id);
    IReadOnlyCollection<ItemCategory> GetCategories();
}
