using System;
using System.Collections.Generic;
using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class DynamicText : RichTextLabel, IEventParser
{
    public DynamicText()
    {
        BbcodeEnabled = true;
        FitContent = true;
        VisibleCharacters = 0;
        VisibleCharactersBehavior = TextServer.VisibleCharactersBehavior.CharsAfterShaping;
    }

    public static string GetScenePath() => GDEx.GetScenePath();
    private double _counter;
    private int _currentLine;
    private string _customText = string.Empty;
    private int _endChar = -1;
    private List<int> _lineBreakCharIndices = new();
    private bool _showToEndCharEnabled;
    private bool _sizeDirty;
    private double _speed = 0.02;
    private double _speedMultiplier = 1;
    private bool _textDirty;
    private List<ITextEvent> _textEvents = new();
    private int _textEventIndex;
    private bool _writeTextEnabled;
    [Export]
    public int CurrentLine
    {
        get => _currentLine;
        set
        {
            _currentLine = GetValidLine(value);
            MoveToLine(_currentLine);
        }
    }
    /// <summary>
    /// The custom text to use for display.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.MultilineText)]
    public string CustomText
    {
        get => _customText;
        set
        {
            _customText = value;
            _textDirty = true;
        }
    }
    [Export]
    public bool ShowToEndCharEnabled
    {
        get => _showToEndCharEnabled;
        set
        {
            _showToEndCharEnabled = value;
            if (value)
            {
                VisibleCharacters = EndChar;
                RaiseTextEvents();
            }
        }
    }
    [Export]
    public bool Writing
    {
        get => CurrentState == State.Writing;
        set
        {
            if (value)
                StartWriting();
            else
                StopWriting();
        }
    }
    [Export]
    public double SpeedMultiplier
    {
        get => _speedMultiplier;
        set
        {
            if (value < 0)
                value = 0;
            if (value > _speedMultiplier)
                Counter = (value * _speed) - (_speedMultiplier * _speed);
            _speedMultiplier = value;
        }
    }

    public State CurrentState { get; private set; }
    public int ContentHeight { get; private set; }
    public int EndChar
    {
        get => _endChar == -1 ? TotalCharacterCount : _endChar;
        set => _endChar = value;
    }
    public int LineCount { get; private set; }
    public double Speed => SpeedUpEnabled ? 0 : SpeedMultiplier * _speed;
    public bool SpeedUpEnabled { get; set; }
    public int TotalCharacterCount { get; private set; }
    private double Counter
    {
        get => _counter;
        set => _counter = Math.Max(value, 0);
    }
    public event Action? LoadingStarted;
    public event Action? StartedWriting;
    public event Action? StoppedWriting;
    public event Action<ITextEvent>? TextEventTriggered;
    public event Action? TextDataUpdated;
    public event Action? TextUpdated;

    public enum State
    {
        Opening,
        Loading,
        Idle,
        Writing
    }

    public override void _Ready() => Init();

    public override void _Process(double delta)
    {
        if (CurrentState == State.Loading)
            return;
        if (_textDirty)
            HandleTextDirty();
        else if (_sizeDirty)
            HandleSizeDirty();
        else if (CurrentState == State.Writing)
        {
            if (IsAtTextEnd())
                StopWriting();
            else
                Write(delta);
        }
    }

    public int GetFirstCharIndexByLine(int line)
    {
        line = Math.Clamp(line, 0, _lineBreakCharIndices.Count - 1);
        return _lineBreakCharIndices[line];
    }

    public bool IsAtTextEnd()
    {
        return VisibleCharacters >= EndChar && Counter == 0;
    }

    public void OnResized() => _sizeDirty = true;

    public void RefreshText() => HandleTextDirty();

    public void ResetSpeed() => SpeedMultiplier = 1;

    public void SetPause(double time) => Counter += time;

    public void SpeedUpText() => Counter = 0;

    public void StartWriting()
    {
        if (CurrentState != State.Idle)
            return;
        CurrentState = State.Writing;
        StartedWriting?.Invoke();
        RaiseTextEvents();
    }

    public void StopWriting()
    {
        if (CurrentState != State.Writing)
            return;
        CurrentState = State.Idle;
        StoppedWriting?.Invoke();
    }

    public void UpdateText(DialogLine dialogLine)
    {
        CurrentState = State.Loading;
        LoadingStarted?.Invoke();
        ParseAndAssignText(dialogLine.Text, dialogLine, _textEvents);
        CurrentState = State.Idle;
        TextUpdated?.Invoke();
    }

    private int GetValidLine(int line) => Math.Clamp(line, 0, LineCount - 1);

    private List<int> GetLineBreakCharIndices()
    {
        List<int> lineBreakCharIndices = new() { 0 };
        int currentLine = 0;
        for (int i = 0; i < TotalCharacterCount; i++)
        {
            int line = GetCharacterLine(i);
            if (line > currentLine)
            {
                currentLine = line;
                lineBreakCharIndices.Add(i - 1);
            }
        }
        return lineBreakCharIndices;
    }

    private float GetLineOffsetOrEnd(int line)
    {
        return line < LineCount ? GetLineOffset(line) : ContentHeight;
    }

    private ITextEvent? GetNextTextEvent()
    {
        if (_textEventIndex >= _textEvents.Count)
            return null;
        return _textEvents[_textEventIndex];
    }

    private void HandleSizeDirty()
    {
        UpdateTextData();
        _sizeDirty = false;
    }

    private void HandleTextDirty()
    {
        bool writeQueued = CurrentState == State.Writing;
        CurrentState = State.Loading;
        LoadingStarted?.Invoke();
        _textEvents = new();
        ParseAndAssignText(_customText, this, _textEvents);
        CurrentState = writeQueued ? State.Writing : State.Idle;
        TextUpdated?.Invoke();
        _textDirty = false;
    }

    private void HandleTextEvent(ITextEvent textEvent)
    {
        if (!textEvent.TryHandleEvent(this))
            TextEventTriggered?.Invoke(textEvent);
    }

    private void Init()
    {
        //ResetSpeed();
        Counter = Speed;
        UpdateTextData();
        Resized += OnResized;
        ThemeChanged += OnResized;
        CurrentState = State.Idle;
    }

    /// <summary>
    /// Positions text and sets VisibleCharacters to beginning of specified line. 
    /// </summary>
    /// <param name="line"></param>
    private void MoveToLine(int line)
    {
        Position = new Vector2(0, -GetLineOffsetOrEnd(line));
        ResetVisibleCharacters();
    }

    private void ParseAndAssignText(string fullText, IEventParser parser, List<ITextEvent> textEvents)
    {
        VisibleCharacters = 0;
        // Assign to parse BBCode (stupid. I know.)
        Text = fullText;
        Text = parser.GetEventParsedText(fullText, GetParsedText(), textEvents);
        UpdateTextData();
    }

    private void RaiseTextEvents()
    {
        ITextEvent? textEvent = GetNextTextEvent();
        while (textEvent != null && textEvent.Index <= VisibleCharacters)
        {
            // TODO: Handle Seen
            textEvent.Seen = true;
            HandleTextEvent(textEvent);
            _textEventIndex++;
            textEvent = GetNextTextEvent();
        }
    }

    /// <summary>
    /// Sets VisibleCharacters to start or end of display.
    /// </summary>
    private void ResetVisibleCharacters()
    {
        VisibleCharacters = _showToEndCharEnabled ? EndChar : _lineBreakCharIndices[_currentLine];
    }

    private void UpdateTextData()
    {
        LineCount = GetLineCount();
        TotalCharacterCount = GetTotalCharacterCount();
        ContentHeight = GetContentHeight();
        _lineBreakCharIndices = GetLineBreakCharIndices();
        ResetVisibleCharacters();
        TextDataUpdated?.Invoke();
    }

    /// <summary>
    /// Writes out the text at a defined pace.
    /// </summary>
    /// <param name="delta"></param>
    private void Write(double delta)
    {
        if (Counter > 0)
        {
            Counter -= delta;
            return;
        }

        VisibleCharacters++;
        Counter = VisibleCharacters < EndChar ? Speed : 0;
        RaiseTextEvents();


        //Counter += delta * Speed;
        //if (Counter > 0)
        //{
        //    int newChars = (int)Counter;
        //    if (GetNextTextEvent() is TextEvent textEvent)
        //        newChars = Math.Min(newChars, textEvent.Index - VisibleCharacters);
        //    VisibleCharacters += SpeedUpEnabled ? newChars : 1;
        //    Counter = VisibleCharacters < EndChar ? Counter % 1 : 0;
        //    RaiseTextEvents();
        //}
    }
}
