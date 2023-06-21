using System;

namespace GameCore.GUI;

public abstract class TextEvent : ITextEvent
{
    protected TextEvent(int index)
    {
        Index = index;
    }

    public bool Seen { get; set; }
    public int Index { get; set; }

    public abstract bool TryHandleEvent(object context);
}

public class InstructionTextEvent : TextEvent
{
    public InstructionTextEvent(int index, ushort[] instructions)
        : base(index)
    {
        Instructions = instructions;
    }

    public ushort[] Instructions { get; set; }

    public override bool TryHandleEvent(object context)
    {
        if (context is not Dialog dialog)
            return false;
        dialog.EvaluateInstructions(Instructions);
        return true;
    }
}

public class SpeedTextEvent : TextEvent
{
    public SpeedTextEvent(int index, double speedMult)
        : base(index)
    {
        SpeedMultiplier = Math.Max(speedMult, 0);
    }

    public double SpeedMultiplier { get; set; }

    public override bool TryHandleEvent(object context)
    {
        if (context is DynamicText dynamicText)
        {
            dynamicText.SpeedMultiplier = SpeedMultiplier;
            return true;
        }
        return false;
    }
}

public class PauseTextEvent : TextEvent
{
    public PauseTextEvent(int index, double time)
        : base(index)
    {
        Time = time;
    }

    public double Time { get; set; }

    public override bool TryHandleEvent(object context)
    {
        if (context is not DynamicText dynamicText)
            return true;
        dynamicText.SetPause(Time);
        return true;
    }
}

public class SpeakerTextEvent : TextEvent
{
    public SpeakerTextEvent(int index, string speakerId, string? name, string? portrait, string? mood)
        : base(index)
    {
        Mood = mood;
        SpeakerId = speakerId;
        Portrait = portrait;
        Name = name;
    }

    public string? Mood { get; set; }
    public string SpeakerId { get; set; }
    public string? Portrait { get; set; }
    public string? Name { get; set; }

    public override bool TryHandleEvent(object context)
    {
        if (context is not Dialog dialog)
            return false;
        dialog.UpdateSpeaker(false, SpeakerId, Name, Portrait, Mood);
        return true;
    }
}
