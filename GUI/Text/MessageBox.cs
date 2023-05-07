using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class MessageBox : MarginContainer
{
    public static string GetScenePath() => GDEx.GetScenePath();
    private string _messageText = string.Empty;
    private HorizontalAlignment _messageAlign;
    private SizeFlags _boxAlign;
    private MarginContainer _messageMargin = null!;
    private Label _message = null!;
    [Export(PropertyHint.MultilineText)]
    public string MessageText
    {
        get => _messageText;
        set
        {
            _messageText = value;
            if (_message != null)
                _message.Text = _messageText;
        }
    }
    [Export(PropertyHint.Enum)]
    public HorizontalAlignment MessageAlign
    {
        get => _messageAlign;
        set
        {
            _messageAlign = value;
            if (_message != null)
                _message.HorizontalAlignment = _messageAlign;
        }
    }
    [Export(PropertyHint.Enum)]
    public SizeFlags BoxAlign
    {
        get => _boxAlign;
        set
        {
            _boxAlign = value;
            if (_message?.AutowrapMode == TextServer.AutowrapMode.Off)
                SizeFlagsHorizontal = _boxAlign;
        }
    }
    private float _maxWidth;

    public override void _Ready()
    {
        Size = Vector2.Zero;
        SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        _messageMargin = GetNode<MarginContainer>("MessageMargin");
        _message = _messageMargin.GetNode<Label>("Message");
        _message.Text = MessageText;
        Resized += OnResized;
    }

    public void SetMaxWidth(Vector2 maxSize) => _maxWidth = maxSize.X;

    private void OnResized() => HandleResize();

    private void HandleResize()
    {
        if (_maxWidth <= 0)
            return;
        if (ShouldEnableAutoWrap())
            EnableAutoWrap();
        else if (ShouldDisableAutoWrap())
            DisableAutoWrap();
    }

    private bool ShouldEnableAutoWrap()
    {
        return Size.X > _maxWidth || _messageMargin.Size.X > _maxWidth;
    }

    private bool ShouldDisableAutoWrap() => _message.GetLineCount() <= 1;

    private void EnableAutoWrap()
    {
        if (_message.AutowrapMode != TextServer.AutowrapMode.Off)
            return;
        _message.AutowrapMode = TextServer.AutowrapMode.Word;
        SizeFlagsHorizontal = SizeFlags.Fill;
    }

    private void DisableAutoWrap()
    {
        if (_message.AutowrapMode == TextServer.AutowrapMode.Off)
            return;
        _message.AutowrapMode = TextServer.AutowrapMode.Off;
        SizeFlagsHorizontal = BoxAlign;
    }
}
