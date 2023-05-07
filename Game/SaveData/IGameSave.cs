using System;

namespace GameCore;

public interface IGameSave
{
    int Id { get; }
    DateTime LastModifiedUtc { get; }
}
