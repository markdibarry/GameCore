using System;
using System.Collections.Generic;
using GameCore.Statistics;

namespace GameCore.Items;

public abstract class AItem : IEquatable<AItem>
{
    protected AItem(string id, ItemCategory itemCategory)
    {
        Id = id;
        ItemCategory = itemCategory;
        MaxStack = 1;
        IsDroppable = false;
        IsSellable = false;
        IsReusable = false;
        UseData = new ItemUseData();
    }

    public string Id { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string ImgPath { get; init; } = string.Empty;
    public bool IsDroppable { get; init; }
    public bool IsReusable { get; init; }
    public bool IsSellable { get; init; }
    public ItemCategory ItemCategory { get; }
    public int MaxStack { get; init; }
    public ICollection<Modifier> Modifiers { get; set; } = Array.Empty<Modifier>();
    public int Price { get; init; }
    public ItemUseData UseData { get; init; }

    public bool Equals(AItem? other)
    {
        if (other == null)
            return false;
        return other.Id == Id;
    }
}
