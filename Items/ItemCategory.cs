using System;

namespace GameCore.Items;

public class ItemCategory : IEquatable<ItemCategory>
{
    public ItemCategory(string id, string displayName, string? abbreviation = null)
    {
        Id = id;
        DisplayName = displayName;
        Abbreviation = abbreviation ?? displayName;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string Abbreviation { get; }

    public bool Equals(ItemCategory? other)
    {
        if (other == null)
            return false;
        return other.Id == Id;
    }

    public override bool Equals(object? obj) => Equals(obj as ItemCategory);

    public override int GetHashCode() => HashCode.Combine(Id);
}
