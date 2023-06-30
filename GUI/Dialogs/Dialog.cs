using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameCore.Exceptions;
using GameCore.Input;
using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

public partial class Dialog : GUILayer
{
    public Dialog(IGUIController guiController, DialogScript dialogScript)
    {
        GUIController = guiController;
        TextStorage = new TextStorage();
        _scriptReader = new(this, dialogScript);
        _dialogBoxScene = GD.Load<PackedScene>(DialogBox.GetScenePath());
        AnchorBottom = 1.0f;
        AnchorRight = 1.0f;
        SpeedMultiplier = 1;
    }

    public static string GetScenePath() => GDEx.GetScenePath();
    private readonly PackedScene _dialogBoxScene;
    private readonly DialogScriptReader _scriptReader;
    public bool Auto { get; set; }
    public IStorageContext TextStorage { get; set; }
    public DialogBox? SecondaryBox { get; set; }
    public DialogBox? FocusedBox { get; set; }
    public bool SpeedUpEnabled { get; set; }
    public double SpeedMultiplier { get; set; }
    public bool SpeechBubbleEnabled { get; set; }
    public bool DualBoxEnabled { get; set; } = true;

    public override void HandleInput(IGUIInputHandler menuInput, double delta)
    {
        if (CurrentState != State.Available || FocusedBox == null)
            return;

        FocusedBox.HandleInput(menuInput, delta);
    }

    public async Task CloseDialogAsync(bool preventAnimation = false, object? data = null)
    {
        await GUIController.CloseLayerAsync(preventAnimation, data);
    }

    public async Task CloseDialogBoxAsync(DialogBox? box)
    {
        if (box == null)
            return;

        await box.TransitionCloseAsync();
        box.TextEventTriggered -= OnTextEventTriggered;
        box.DialogLineFinished -= OnDialogLineFinished;
        RemoveChild(box);
        box.QueueFree();
    }

    public async Task CloseDialogBoxesAsync()
    {
        await CloseDialogBoxAsync(SecondaryBox);
        await CloseDialogBoxAsync(FocusedBox);
    }

    public void EvaluateInstructions(ushort[] instructions) => _scriptReader.Evaluate(instructions);

    public Speaker GetSpeaker(string speakerId) => _scriptReader.GetSpeaker(speakerId);

    public DialogBox? GetBoxBySpeakerId(string speakerId)
    {
        if (FocusedBox != null && FocusedBox.HasSpeaker(speakerId))
            return FocusedBox;
        else if (SecondaryBox != null && SecondaryBox.HasSpeaker(speakerId))
            return SecondaryBox;
        return null;
    }

    public static DialogScript LoadScript(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new DialogException("No path provided");

        DialogScript? dialogScript = DialogLoader.Load(path);

        if (dialogScript == null || !dialogScript.Sections.Any())
            throw new DialogException($"No dialog found at path: {path}");

        return dialogScript;
    }

    public async Task OpenOptionBoxAsync(List<Choice> choices)
    {
        await GUIController.OpenMenuAsync(scenePath: DialogOptionMenu.GetScenePath(), data: choices);
    }

    public async Task ProceedAsync()
    {
        try
        {
            await _scriptReader.ReadNextStatementAsync(FocusedBox!.DialogLine.Next);
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex);
            await CloseDialogAsync();
        }
    }

    public async Task StartDialogAsync()
    {
        CurrentState = State.Available;
        await _scriptReader.ReadScriptAsync();
    }

    public override void UpdateData(object? data)
    {
        if (data is not List<Choice> choices)
            return;
        _ = _scriptReader.ReadNextStatementAsync(choices[0].Next);
    }

    /// <summary>
    /// Updates the Speaker's name, portrait, and mood
    /// </summary>
    /// <param name="global"></param>
    /// <param name="speakerId"></param>
    /// <param name="displayName"></param>
    /// <param name="portraitId"></param>
    /// <param name="mood"></param>
    public void UpdateSpeaker(bool global, string speakerId, string? displayName, string? portraitId, string? mood)
    {
        Speaker speaker = GetSpeaker(speakerId);
        DialogBox? dialogBox = GetBoxBySpeakerId(speakerId);
        Portrait? portrait = dialogBox?.GetPortrait(speakerId);

        if (displayName != null)
        {
            speaker.DisplayName = displayName;
            dialogBox?.UpdateSpeakersNames();
        }

        if (portraitId != null)
        {
            speaker.Portrait = portraitId;
            portrait?.SetPortraitFrames(speaker.Portrait);
        }

        if (mood != null)
        {
            if (global)
            {
                speaker.GlobalMood = mood;
            }
            else
            {
                speaker.Mood = mood;
                portrait?.SetMood(speaker.Mood);
            }
        }
    }

    public override Task CloseAsync(bool preventAnimation = false)
    {
        CurrentState = State.Closing;
        CurrentState = State.Closed;
        return Task.CompletedTask;
    }

    private void OnTextEventTriggered(ITextEvent textEvent)
    {
        textEvent.TryHandleEvent(this);
    }

    private void OnDialogLineFinished()
    {
        _ = ProceedAsync();
    }

    private DialogBox CreateDialogBox(bool displayRight)
    {
        DialogBox newBox = _dialogBoxScene.Instantiate<DialogBox>();
        newBox.TextEventTriggered += OnTextEventTriggered;
        newBox.DialogLineFinished += OnDialogLineFinished;
        newBox.DisplayRight = displayRight;
        AddChild(newBox);
        newBox.SpeedMultiplier = SpeedMultiplier;
        return newBox;
    }

    public async Task HandleLineAsync(DialogLine newLine)
    {
        if (SpeechBubbleEnabled)
            throw new DialogException("Speech bubble not implemented.");

        if (DualBoxEnabled)
            await HandleDualBoxLineAsync(newLine);
        else
            await HandleSingleBoxLineAsync(newLine);
    }

    public async Task HandleSingleBoxLineAsync(DialogLine newLine)
    {
        if (SecondaryBox != null)
        {
            await CloseDialogBoxAsync(SecondaryBox);
            SecondaryBox = null;
        }

        // If no box exists, make one.
        if (FocusedBox == null)
        {
            await OpenNewDialogBoxAsync(newLine, false);
            return;
        }

        // Try to reuse existing box
        if (await TryUseDialogBoxAsync(newLine, FocusedBox))
            return;

        await CloseDialogBoxAsync(FocusedBox);
        await OpenNewDialogBoxAsync(newLine, false);
        return;
    }

    public async Task HandleDualBoxLineAsync(DialogLine newLine)
    {
        // If no box exists, make one.
        if (FocusedBox == null)
        {
            await OpenNewDialogBoxAsync(newLine, false);
            return;
        }

        // Try to reuse existing box
        if (await TryUseDialogBoxAsync(newLine, FocusedBox))
            return;

        bool displayRight = FocusedBox.DisplayRight;

        // Close if contains overlapping speakers
        if (newLine.AnySpeakers(FocusedBox.DialogLine))
        {
            await CloseDialogBoxAsync(FocusedBox);
            FocusedBox = null;
        }
        else
        {
            FocusedBox.Dim = true;
        }

        // First box can't be used, so swap them
        (FocusedBox, SecondaryBox) = (SecondaryBox, FocusedBox);

        // If no second box exists, make one
        if (FocusedBox == null)
        {
            await OpenNewDialogBoxAsync(newLine, !displayRight);
            return;
        }

        // Try to reuse existing second box
        if (await TryUseDialogBoxAsync(newLine, FocusedBox))
            return;

        await CloseDialogBoxAsync(FocusedBox);
        await OpenNewDialogBoxAsync(newLine, !displayRight);
    }

    private async Task OpenNewDialogBoxAsync(DialogLine dialogLine, bool displayRight)
    {
        FocusedBox = CreateDialogBox(displayRight);
        await FocusedBox.UpdateDialogLine(dialogLine);
        FocusedBox.StartWriting();
    }

    private async Task<bool> TryUseDialogBoxAsync(DialogLine dialogLine, DialogBox box)
    {
        if (!dialogLine.SameSpeakers(box.DialogLine))
            return false;
        box.MoveToFront();
        await box.UpdateDialogLine(dialogLine);
        box.SpeedMultiplier = SpeedMultiplier;
        box.Dim = false;
        box.StartWriting();
        return true;
    }
}
