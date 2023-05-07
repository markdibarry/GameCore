using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.Actors;

namespace GameCore.Items;

public class ItemStack
{
    public ItemStack(AItem item, int amount)
    {
        _reservations = new List<Reservation>();
        Item = item;
        Count = Math.Clamp(amount, 1, item.MaxStack);
    }

    private readonly IList<Reservation> _reservations;
    public int Count { get; private set; }
    public AItem Item { get; }
    public IReadOnlyCollection<Reservation> Reservations => _reservations.ToList();

    public bool CanReserve() => _reservations.Count < Count;

    /// <summary>
    /// Adds the amount requested to the stack Count.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>The amount that could not be added.</returns>
    public int AddAmount(int amount)
    {
        if (amount <= 0)
            return 0;
        if (Count + amount <= Item.MaxStack)
        {
            Count += amount;
            return 0;
        }
        int leftOver = amount + Count - Item.MaxStack;
        Count = Item.MaxStack;
        return leftOver;
    }

    /// <summary>
    /// Removes the amount requested of the non-reserved count.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>The amount that could not be removed.</returns>
    public int RemoveAmount(int amount)
    {
        if (amount <= 0)
            return 0;
        int removableCount = Count - _reservations.Count;
        if (amount < removableCount)
        {
            Count -= amount;
            return 0;
        }
        int leftOver = amount - removableCount;
        Count = _reservations.Count;
        return leftOver;
    }

    public void AddReservation(AActor actor, EquipmentSlot slot)
    {
        if (_reservations.Any(x => x.EquipmentSlot == slot))
            return;
        _reservations.Add(new Reservation(actor, slot));
    }

    public void RemoveReservation(EquipmentSlot slot)
    {
        Reservation? reservation = _reservations.FirstOrDefault(x => x.EquipmentSlot == slot);
        if (reservation != null)
            _reservations.Remove(reservation);
    }
}
