using System;
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

        foreach (Modifier mod in ItemStack.Item.Modifiers)
            actor.Stats.RemoveMod(mod, this);

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

        foreach (Modifier mod in ItemStack.Item.Modifiers)
            actor.Stats.AddMod(mod, this);

        return true;
    }
}
