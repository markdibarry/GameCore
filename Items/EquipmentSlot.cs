using System;
using System.Collections.Generic;
using GameCore.Actors;
using GameCore.Statistics;

namespace GameCore.Items;

public class EquipmentSlot
{
    public EquipmentSlot(EquipmentSlotCategory category)
    : this(category, null)
    {
    }

    public EquipmentSlot(EquipmentSlotCategory category, ItemStack? item)
    {
        SlotCategory = category;
        ItemStack = item;
    }

    public EquipmentSlotCategory SlotCategory { get; }
    public ItemStack? ItemStack { get; set; }
    public List<Modifier> Modifiers { get; } = new();
    public BaseItem? Item => ItemStack?.Item;

    public bool IsCompatible(BaseItem item)
    {
        return Array.IndexOf(SlotCategory.ItemCategoryIds, item.ItemCategory.Id) != -1;
    }

    public void RemoveItem(BaseActor actor)
    {
        if (ItemStack == null)
            return;
        ItemStack.RemoveReservation(this);
        RemoveModifiersFromStats(actor.Stats);
        Modifiers.Clear();
        ItemStack = null;
    }

    public bool TrySetItem(BaseActor actor, ItemStack newItemStack)
    {
        if (!newItemStack.CanReserve())
            return false;
        if (!IsCompatible(newItemStack.Item))
            return false;

        RemoveItem(actor);

        newItemStack.AddReservation(actor, this);
        ItemStack = newItemStack;
        CloneItemModifiers(ItemStack.Item);
        AddModifiersToStats(actor.Stats);
        return true;
    }

    private void AddModifiersToStats(BaseStats stats)
    {
        foreach (Modifier mod in Modifiers)
            stats.AddMod(mod);
    }

    private void CloneItemModifiers(BaseItem item)
    {
        foreach (Modifier mod in item.Modifiers)
            Modifiers.Add(new Modifier(mod));
    }

    private void RemoveModifiersFromStats(BaseStats stats)
    {
        foreach (Modifier mod in Modifiers)
            stats.RemoveMod(mod);
    }
}
