using System;
using Godot;

namespace GameCore.GUI;

public partial class Portrait : AnimatedSprite2D
{
    public void SetMood(string newMood)
    {
        if (!SpriteFrames.HasAnimation(newMood.ToLower()))
            throw new NotImplementedException($"No portrait found for {Name} {newMood}");
        Play(newMood);
    }

    public void SetPortraitFrames(string newPortraitName)
    {
        string path = $"{Config.PortraitsFullPath}/{newPortraitName.ToLower()}/portraits.tres";
        SpriteFrames = GD.Load<SpriteFrames>(path);
    }
}
