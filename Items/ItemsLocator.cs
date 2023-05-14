using System;
using System.Collections.Generic;

namespace GameCore.Items;

public static class ItemsLocator
{
    private static IEquipmentSlotCategoryDB? s_equipmentSlotCategoryDB;
    private static readonly NullEquipmentSlotCategoryDB s_nullEquipmentSlotCategoryDB = new();
    private static IItemDB? s_itemDB;
    private static readonly NullItemDB s_nullItemDB = new();
    private static IItemCategoryDB? s_itemCategoryDB;
    private static readonly NullItemCategoryDB s_nullItemCategoryDB = new();
    public static IEquipmentSlotCategoryDB EquipmentSlotCategoryDB => s_equipmentSlotCategoryDB ?? s_nullEquipmentSlotCategoryDB;
    public static IItemDB ItemDB => s_itemDB ?? s_nullItemDB;
    public static IItemCategoryDB ItemCategoryDB => s_itemCategoryDB ?? s_nullItemCategoryDB;

    public static void ProvideEquipmentSlotCategoryDB(IEquipmentSlotCategoryDB equipmentSlotCategoryDB)
    {
        s_equipmentSlotCategoryDB = equipmentSlotCategoryDB;
    }

    public static void ProvideItemDB(IItemDB itemDB) => s_itemDB = itemDB;

    public static void ProvideItemCategoryDB(IItemCategoryDB itemCategoryDB) => s_itemCategoryDB = itemCategoryDB;

    private class NullEquipmentSlotCategoryDB : IEquipmentSlotCategoryDB
    {
        public EquipmentSlotCategory? GetCategory(string id) => null;

        public IReadOnlyCollection<EquipmentSlotCategory> GetCategoryPreset(string id) => Array.Empty<EquipmentSlotCategory>();
    }

    private class NullItemDB : IItemDB
    {
        public BaseItem? GetItem(string id) => null;

        public IEnumerable<BaseItem> GetItemsByCategory(string itemCategoryId) => Array.Empty<BaseItem>();
    }

    private class NullItemCategoryDB : IItemCategoryDB
    {
        public IReadOnlyCollection<ItemCategory> GetCategories() => Array.Empty<ItemCategory>();
        public ItemCategory? GetCategory(string id) => null;
    }
}
