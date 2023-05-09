namespace GameCore.ActionEffects;

public interface IActionEffectDB
{
    IActionEffect? GetEffect(int type);
}
