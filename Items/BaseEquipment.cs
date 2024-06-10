using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.Actors;

namespace GameCore.Items;

public class BaseEquipment
{
    public BaseEquipment(BaseInventory inventory, IEnumerable<EquipmentSlotCategory> categories)
    {
        _inventory = inventory;
        _slots = categories.Select(x => new EquipmentSlot(x)).ToArray();
    }

    public BaseEquipment(BaseInventory inventory, EquipmentSlot[] slots)
    {
        _inventory = inventory;
        _slots = slots;
    }

    private readonly BaseInventory _inventory;
    private readonly EquipmentSlot[] _slots;
    public IReadOnlyCollection<EquipmentSlot> Slots => _slots;
    public Action<EquipmentSlot, BaseItem?, BaseItem?>? EquipmentSetCallback { get; set; }

    public IEnumerable<EquipmentSlot> GetSlotsByType(string itemCategoryId)
    {
        return _slots.Where(x => x.SlotCategory.ItemCategoryIds.Contains(itemCategoryId));
    }

    public EquipmentSlot? GetSlot(string slotCategoryId)
    {
        return _slots.FirstOrDefault(x => x.SlotCategory.Id.Equals(slotCategoryId));
    }

    public bool TrySetItem(BaseActor actor, EquipmentSlot slot, BaseItem item)
    {
        ItemStack? itemStack = _inventory.GetItemStacks(item)
            .FirstOrDefault(x => x.CanReserve());

        if (itemStack == null)
            return false;

        return TrySetItem(actor, slot, itemStack);
    }

    public bool TrySetItem(BaseActor actor, EquipmentSlot slot, ItemStack newItemStack)
    {
        BaseItem? oldItem = slot.Item;

        if (!slot.TrySetItem(actor, newItemStack))
            return false;

        EquipmentSetCallback?.Invoke(slot, oldItem, newItemStack.Item);
        return true;
    }

    public void RemoveItem(BaseActor actor, EquipmentSlot slot)
    {
        if (slot.ItemStack == null)
            return;

        BaseItem? oldItem = slot.Item;
        slot.RemoveItem(actor);
        EquipmentSetCallback?.Invoke(slot, oldItem, null);
    }
}
