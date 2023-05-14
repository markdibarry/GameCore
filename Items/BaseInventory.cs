using System.Collections.Generic;
using System.Linq;

namespace GameCore.Items;

public abstract class BaseInventory
{
    protected BaseInventory()
    {
        _itemStacks = new();
    }

    protected BaseInventory(IEnumerable<ItemStack> itemStacks)
    {
        _itemStacks = itemStacks.ToList();
    }

    private readonly List<ItemStack> _itemStacks;
    public IReadOnlyCollection<ItemStack> Items => _itemStacks;
    public bool AllowMultipleStacks { get; }

    /// <summary>
    /// Returns the matching ItemStacks for Item Id provided. Returns null if no stack is found.
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns>ItemStack</returns>
    public IReadOnlyCollection<ItemStack> GetItemStacks(string itemId)
    {
        return _itemStacks.Where(x => x.Item.Id.Equals(itemId)).ToList();
    }

    /// <summary>
    /// Returns the matching ItemStacks for item provided. Returns null if no stack is found.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public IReadOnlyCollection<ItemStack> GetItemStacks(BaseItem item) => GetItemStacks(item.Id);

    /// <summary>
    /// Returns ItemStacks matching the category Id provided.
    /// </summary>
    /// <param name="itemCategoryId"></param>
    /// <returns></returns>
    public IReadOnlyCollection<ItemStack> GetItemsByType(string itemCategoryId)
    {
        return _itemStacks.Where(x => x.Item.ItemCategory.Id == itemCategoryId).ToList();
    }

    /// <summary>
    /// Adds the amount requested to any available item stacks.
    /// If not enough room is available, a new stack is created to contain them.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amount"></param>
    /// <returns>The number of items unable to be added to the inventory.</returns>
    public int AddItem(BaseItem item, int amount = 1)
    {
        int leftOver = amount;
        IReadOnlyCollection<ItemStack> itemStacks = GetItemStacks(item);

        // if multiple stacks aren't allowed, there should be only one.
        foreach (var stack in itemStacks)
        {
            if (leftOver == 0)
                break;
            leftOver = stack.AddAmount(leftOver);
        }

        if (leftOver == 0)
            return 0;

        if (itemStacks.Any() && !AllowMultipleStacks)
            return leftOver;

        if (leftOver <= item.MaxStack)
        {
            _itemStacks.Add(new ItemStack(item, leftOver));
            return 0;
        }

        if (!AllowMultipleStacks)
        {
            _itemStacks.Add(new ItemStack(item, item.MaxStack));
            return leftOver - item.MaxStack;
        }

        while (leftOver > item.MaxStack)
        {
            _itemStacks.Add(new ItemStack(item, item.MaxStack));
            leftOver -= item.MaxStack;
        }
        _itemStacks.Add(new ItemStack(item, leftOver));
        return 0;
    }

    /// <summary>
    /// Adds the amount requested to the requested item stack.
    /// If not enough room is available, a new stack is created to contain them.
    /// </summary>
    /// <param name="itemStack"></param>
    /// <param name="amount"></param>
    /// <returns>The number of items unable to be added to the inventory.</returns>
    public int AddItem(ItemStack itemStack, int amount = 1)
    {
        if (!_itemStacks.Contains(itemStack))
            return amount;
        int leftOver = itemStack.AddAmount(amount);
        if (leftOver > 0)
            leftOver = AddItem(itemStack.Item, leftOver);
        return leftOver;
    }

    /// <summary>
    /// Removes amount requested from any available item stacks.
    /// If more is needed removed, it will search for more stacks and remove items
    /// from them as well.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amount"></param>
    /// <returns>The number of items unable to be removed from the inventory.</returns>
    public int RemoveItem(BaseItem item, int amount = 1)
    {
        int leftOver = amount;
        IReadOnlyCollection<ItemStack> itemStacks = GetItemStacks(item);
        foreach (var stack in itemStacks)
        {
            if (leftOver == 0)
                break;
            leftOver = stack.RemoveAmount(leftOver);
        }
        _itemStacks.RemoveAll(x => x.Count == 0);
        return leftOver;
    }

    /// <summary>
    /// Removes amount requested from the requested item stack.
    /// If more is needed removed, it will search for more stacks and remove items
    /// from them as well.
    /// </summary>
    /// <param name="itemStack"></param>
    /// <param name="amount"></param>
    /// <returns>The number of items unable to be removed from the inventory.</returns>
    public int RemoveItem(ItemStack itemStack, int amount = 1)
    {
        if (!_itemStacks.Contains(itemStack))
            return amount;
        int leftOver = itemStack.RemoveAmount(amount);
        if (leftOver > 0)
            _itemStacks.Remove(itemStack);
        return RemoveItem(itemStack.Item, leftOver);
    }
}
