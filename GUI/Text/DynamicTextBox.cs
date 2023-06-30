using System;
using System.Collections.Generic;
using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class DynamicTextBox : Control
{
    public static string GetScenePath() => GDEx.GetScenePath();
    private int _currentPage;
    private float _displayHeight;
    private DynamicText _dynamicText = GDEx.Instantiate<DynamicText>(DynamicText.GetScenePath());
    private IList<int> _pageBreakLineIndices = new[] { 0 };
    private Control _textWindow = new();
    [Export]
    public int CurrentPage
    {
        get => _currentPage;
        set => MoveToPage(GetValidPage(value));
    }
    [Export(PropertyHint.MultilineText)]
    public string CustomText
    {
        get => _dynamicText.CustomText;
        set => _dynamicText.CustomText = value;
    }
    [Export]
    public bool ShowToEndCharEnabled
    {
        get => _dynamicText.ShowToEndCharEnabled;
        set => _dynamicText.ShowToEndCharEnabled = value;
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
        get => _dynamicText.SpeedMultiplier;
        set => _dynamicText.SpeedMultiplier = value;
    }
    [Export]
    public bool Reset
    {
        get => false;
        set
        {
            if (value)
                _dynamicText.ResetText();
        }
    }
    public State CurrentState { get; private set; }
    public bool SpeedUpEnabled
    {
        get => _dynamicText.SpeedUpEnabled;
        set => _dynamicText.SpeedUpEnabled = value;
    }
    public event Action<ITextEvent>? TextEventTriggered;
    public event Action? StartedWriting;
    public event Action? StoppedWriting;
    public enum State
    {
        Opening,
        Loading,
        Idle,
        Writing
    }

    public override void _Ready() => Init();

    public bool IsAtLastPage() => CurrentPage == _pageBreakLineIndices.Count - 1;

    public bool IsAtPageEnd() => _dynamicText.IsAtTextEnd();

    public void ResetText()
    {
        CurrentPage = 0;
        _dynamicText.ResetText();
    }

    public void SetPause(double time) => _dynamicText.SetPause(time);

    public void StartWriting()
    {
        if (CurrentState != State.Idle)
            return;
        _dynamicText.StartWriting();
    }

    public void StopWriting()
    {
        if (CurrentState != State.Writing)
            return;
        _dynamicText.StopWriting();
    }

    public void UpdateText(DialogLine dialogLine)
    {
        _dynamicText.UpdateText(dialogLine);
    }

    private int GetEndChar(int page)
    {
        if (page + 1 >= _pageBreakLineIndices.Count)
            return _dynamicText.TotalCharacterCount;
        return GetFirstCharIndexByPage(page + 1);
    }

    private int GetFirstCharIndexByPage(int page)
    {
        page = GetValidPage(page);
        int line = _pageBreakLineIndices[page];
        return _dynamicText.GetFirstCharIndexByLine(line);
    }

    private IList<int> GetPageBreakLineIndices()
    {
        List<int> pageBreaksLineIndices = new() { 0 };
        int totalLines = _dynamicText.LineCount;
        float startLineOffset = 0;
        float currentLineOffset = 0;
        float nextLineOffset;
        float newHeight;
        for (int i = 0; i < totalLines; i++)
        {
            if (i + 1 < totalLines)
                nextLineOffset = _dynamicText.GetLineOffset(i + 1);
            else
                nextLineOffset = _dynamicText.ContentHeight;
            newHeight = nextLineOffset - startLineOffset;
            if (newHeight > _displayHeight)
            {
                pageBreaksLineIndices.Add(i);
                startLineOffset = currentLineOffset;
            }
            currentLineOffset = nextLineOffset;
        }
        return pageBreaksLineIndices;
    }

    private int GetValidPage(int page) => Math.Clamp(page, 0, _pageBreakLineIndices.Count - 1);

    private void Init()
    {
        AddChild(_textWindow);
        _textWindow.AddChild(_dynamicText);
        _dynamicText.AnchorsPreset = (int)LayoutPreset.FullRect;
        SubscribeEvents();
        CurrentState = State.Idle;
    }

    private void OnLoadingStarted()
    {
        CurrentState = State.Loading;
    }

    private void OnResized()
    {
        _dynamicText.SetDeferred(nameof(Size), new Vector2(_textWindow.Size.X, _dynamicText.Size.Y));
        _dynamicText.OnResized();
    }

    private void OnStartedWriting()
    {
        CurrentState = State.Writing;
        StartedWriting?.Invoke();
    }

    private void OnStoppedWriting()
    {
        CurrentState = State.Idle;
        StoppedWriting?.Invoke();
    }

    private void OnTextDataUpdated()
    {
        UpdateTextData();
    }

    private void OnTextUpdated()
    {
        CurrentState = State.Idle;
    }

    private void OnTextEventTriggered(ITextEvent textEvent)
    {
        if (!textEvent.TryHandleEvent(this))
            TextEventTriggered?.Invoke(textEvent);
    }

    private void SubscribeEvents()
    {
        _dynamicText.LoadingStarted += OnLoadingStarted;
        _dynamicText.StartedWriting += OnStartedWriting;
        _dynamicText.StoppedWriting += OnStoppedWriting;
        _dynamicText.TextEventTriggered += OnTextEventTriggered;
        _dynamicText.TextDataUpdated += OnTextDataUpdated;
        _dynamicText.TextUpdated += OnTextUpdated;
        _textWindow.Resized += OnResized;
    }

    /// <summary>
    /// Positions text and sets VisibleCharacters to beginning of specified page. 
    /// </summary>
    /// <param name="page"></param>
    private void MoveToPage(int page)
    {
        _currentPage = page;
        _dynamicText.EndChar = GetEndChar(page);
        _dynamicText.CurrentLine = _pageBreakLineIndices[page];
    }

    private void UpdateTextData()
    {
        _displayHeight = _textWindow.Size.Y;
        _pageBreakLineIndices = GetPageBreakLineIndices();
        CurrentPage = 0;
    }
}
