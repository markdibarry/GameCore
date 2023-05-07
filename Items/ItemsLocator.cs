using System.Collections.Generic;

namespace GameCore.Items;

public static class ItemsLocator
{
    private static AEquipmentSlotCategoryDB s_equipmentSlotCategoryDB = null!;
    private static AItemDB s_itemDB = null!;
    private static AItemCategoryDB s_itemCategoryDB = null!;
    public static AEquipmentSlotCategoryDB EquipmentSlotCategoryDB => s_equipmentSlotCategoryDB;
    public static AItemDB ItemDB => s_itemDB;
    public static AItemCategoryDB ItemCategoryDB => s_itemCategoryDB;

    public static void ProvideEquipmentSlotCategoryDB(AEquipmentSlotCategoryDB equipmentSlotCategoryDB)
    {
        s_equipmentSlotCategoryDB = equipmentSlotCategoryDB;
    }

    public static void ProvideItemDB(AItemDB itemDB) => s_itemDB = itemDB;

    public static void ProvideItemCategoryDB(AItemCategoryDB itemCategoryDB) => s_itemCategoryDB = itemCategoryDB;

    public static List<string> CheckReferences()
    {
        List<string> unsetRefs = new();
        if (s_equipmentSlotCategoryDB == null)
            unsetRefs.Add("EquipmentSlotCategory DB");
        if (s_itemCategoryDB == null)
            unsetRefs.Add("ItemCategory DB");
        if (s_itemDB == null)
            unsetRefs.Add("Item DB");
        return unsetRefs;
    }
}
