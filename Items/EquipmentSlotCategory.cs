using System;

namespace GameCore.Items;

public class EquipmentSlotCategory : IEquatable<EquipmentSlotCategory>
{
    public EquipmentSlotCategory(string id, string category, string displayName, string? abbreviation)
        : this(id, new string[] { category }, displayName, abbreviation)
    {
    }

    public EquipmentSlotCategory(string id, string[] categories, string displayName, string? abbreviation)
    {
        Id = id;
        ItemCategoryIds = categories;
        DisplayName = displayName;
        Abbreviation = abbreviation ?? displayName;
    }

    /// <summary>
    /// Identifier for category
    /// </summary>
    public string Id { get; }
    /// <summary>
    /// Compatible ItemCategory Ids
    /// </summary>
    public string[] ItemCategoryIds { get; }
    /// <summary>
    /// Display name for use in menus
    /// </summary>
    public string DisplayName { get; }
    /// <summary>
    /// Abbreviation of display name for use in menus
    /// </summary>
    public string Abbreviation { get; }

    public bool Equals(EquipmentSlotCategory? other)
    {
        if (other == null)
            return false;
        return Id == other.Id;
    }
}
