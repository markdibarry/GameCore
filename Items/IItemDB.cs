using System.Collections.Generic;

namespace GameCore.Items;

public interface IItemDB
{
    BaseItem? GetItem(string id);
    IEnumerable<BaseItem> GetItemsByCategory(string itemCategoryId);
}
