using System.Threading.Tasks;

namespace GameCore.GUI;

public interface IGUIController
{
    Task CloseLayerAsync(bool preventAnimation = false, object? data = null);
    Task CloseAllLayersAsync(bool preventAnimation = false);
    Task OpenMenuAsync(string scenePath, bool preventAnimation = false, object? data = null);
}
