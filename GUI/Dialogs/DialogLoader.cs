using System.IO;
using System.Text.Json;
using GameCore.Exceptions;

namespace GameCore.GUI;

public static class DialogLoader
{
    public static DialogScript Load(string path)
    {
        path = Godot.ProjectSettings.GlobalizePath(path);
        //path = Godot.ProjectSettings.GlobalizePath($"{Config.DialogPath}{path}.json");
        string content = File.ReadAllText(path);
        DialogScript? dialogScript = JsonSerializer.Deserialize<DialogScript>(content);

        if (dialogScript == null)
            throw new DialogException("Could not deserialize.");

        return dialogScript;
    }
}
