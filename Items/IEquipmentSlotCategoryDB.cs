using System.Collections.Generic;

namespace GameCore.Items;

public interface IEquipmentSlotCategoryDB
{
    EquipmentSlotCategory? GetCategory(string id);
    IReadOnlyCollection<EquipmentSlotCategory> GetCategoryPreset(string id);
}
