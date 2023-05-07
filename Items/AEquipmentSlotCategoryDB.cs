using System;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Items;

public abstract class AEquipmentSlotCategoryDB
{
    protected AEquipmentSlotCategoryDB()
    {
        _categories = BuildDB();
        _presets = BuildPresetDB();
    }

    private readonly EquipmentSlotCategory[] _categories;
    private readonly Dictionary<string, string[]> _presets;
    public IReadOnlyCollection<EquipmentSlotCategory> Categories => _categories;
    public IReadOnlyDictionary<string, string[]> Presets => _presets;

    public EquipmentSlotCategory? GetCategory(string id)
    {
        return Array.Find(_categories, category => category.Id.Equals(id));
    }

    public IReadOnlyCollection<EquipmentSlotCategory> GetCategoryPreset(string id)
    {
        if (_presets.TryGetValue(id, out string[]? preset))
            return preset.Select(x => GetCategory(x)).OfType<EquipmentSlotCategory>().ToList();
        return Array.Empty<EquipmentSlotCategory>();
    }

    protected abstract EquipmentSlotCategory[] BuildDB();

    protected abstract Dictionary<string, string[]> BuildPresetDB();
}
