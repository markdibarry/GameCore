namespace GameCore.GUI;

public interface ITextEvent
{
    bool Seen { get; set; }
    int Index { get; set; }
    bool TryHandleEvent(object context) => true;
}
