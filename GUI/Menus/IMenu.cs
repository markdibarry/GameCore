using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace GameCore.GUI;

public interface IMenu
{
    Stack<SubMenu> SubMenus { get; }
    Task CloseSubMenuAsync(Type? cascadeTo = null, bool preventAnimation = false, object? data = null);
    Task HideInactiveSubMenus(float fade = 0);
    Task ShowInactiveSubMenus(float fade = 0);
    Task OpenSubMenuAsync(string path, bool preventAnimation = false, object? data = null);
    Task OpenSubMenuAsync(PackedScene packedScene, bool preventAnimation = false, object? data = null);
}
