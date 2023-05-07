namespace GameCore.GUI;

public partial class DialogBridge : DialogBridgeBase
{
    public float Gold { get; set; }
    public bool IsDay { get; set; }

    public bool IsCool(string name, bool aBool = false)
    {
        return false;
    }

    public override string GetName(string name)
    {
        return base.GetName(name);
    }
}
