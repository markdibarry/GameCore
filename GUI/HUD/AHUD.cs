﻿using System.Collections.Generic;
using GameCore.Actors;
using Godot;

namespace GameCore.GUI;

public abstract partial class AHUD : CanvasLayer
{
    protected Queue<string> MessageQueue { get; set; } = new();
    protected MessageBoxList MessageBoxList { get; set; } = null!;

    public override void _Ready()
    {
        MessageBoxList = GetNode<MessageBoxList>("%MessageBoxList");
    }

    public override void _Process(double delta)
    {
        ProcessQueue();
    }

    public virtual void Pause()
    {
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public virtual void Resume()
    {
        ProcessMode = ProcessModeEnum.Inherit;
    }

    public abstract void SubscribeActorBodyEvents(AActorBody actorBody);

    public abstract void UnsubscribeActorBodyEvents(AActorBody actorBody);

    protected void ProcessQueue()
    {
        if (MessageQueue.Count == 0)
            return;
        MessageBoxList.AddMessageToTop(MessageQueue.Dequeue());
    }
}
