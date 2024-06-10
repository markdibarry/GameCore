using System;
using System.Collections.Generic;
using GameCore.Statistics;

namespace GameCore.Items;

public abstract class BaseItem : IEquatable<BaseItem>
{
    protected BaseItem(string id, ItemCategory itemCategory)
    {
        Id = id;
        ItemCategory = itemCategory;
    }

    public string Id { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string ImgPath { get; init; } = string.Empty;
    public bool IsDroppable { get; init; }
    public bool IsReusable { get; init; }
    public bool IsSellable { get; init; }
    public ItemCategory ItemCategory { get; }
    public int MaxStack { get; init; } = 1;
    public ICollection<Modifier> Modifiers { get; set; } = Array.Empty<Modifier>();
    public ICollection<string> StatusEffects { get; set; } = Array.Empty<string>();
    public int Price { get; init; }
    public ItemUseData UseData { get; init; } = new();

    public bool Equals(BaseItem? other)
    {
        if (other == null)
            return false;

        return other.Id == Id;
    }
}
