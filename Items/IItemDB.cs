using System.Collections.Generic;

namespace GameCore.Items;

public interface IItemDB
{
    AItem? GetItem(string id);
    IEnumerable<AItem> GetItemsByCategory(string itemCategoryId);
}
