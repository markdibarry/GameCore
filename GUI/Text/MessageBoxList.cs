using GameCore.Utility;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class MessageBoxList : VBoxContainer
{
    public static string GetScenePath() => GDEx.GetScenePath();
    public bool IsReady { get; set; }
    private PackedScene _timedMessageBoxScene = GD.Load<PackedScene>(TimedMessageBox.GetScenePath());

    public override void _Ready()
    {
        Resized += OnResized;
        ChildEnteredTree += OnChildEnteredTree;
        foreach (MessageBox messageBox in this.GetChildren<MessageBox>())
            messageBox.SetMaxWidth(Size);
    }

    public void AddMessageToTop(string message)
    {
        var newMessage = _timedMessageBoxScene.Instantiate<TimedMessageBox>();
        newMessage.MessageText = message;
        AddMessageToTop(newMessage);
    }

    public void AddMessageToTop(MessageBox messageBox)
    {
        AddChild(messageBox);
        MoveChild(messageBox, 0);
    }

    public void OnChildEnteredTree(Node node)
    {
        if (node is not MessageBox messageBox)
            return;
        messageBox.SetMaxWidth(Size);
    }

    private void OnResized()
    {
        foreach (MessageBox messageBox in this.GetChildren<MessageBox>())
            messageBox.SetMaxWidth(Size);
    }
}
