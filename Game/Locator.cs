using System.Collections.Generic;
using GameCore.ActionEffects;
using GameCore.Audio;
using GameCore.GUI;
using Godot;

namespace GameCore;

public static class Locator
{
    private static bool s_initialized;
    private static AGameRoot s_gameRoot = null!;
    private static ILoaderFactory s_loaderFactory = null!;
    private static AActionEffectDB s_actionEffectDB = null!;

    public static bool Initialized => s_initialized;
    public static AActionEffectDB ActionEffectDB => s_actionEffectDB;
    public static AAudioController Audio => s_gameRoot.AudioController;
    public static ILoaderFactory LoaderFactory => s_loaderFactory;
    public static AGameRoot Root => s_gameRoot;
    public static AGameSession? Session => s_gameRoot?.GameSession;
    public static ATransitionController TransitionController => s_gameRoot.TransitionController;

    public static void SetInitialized() => s_initialized = true;

    public static void ProvideActionEffectDB(AActionEffectDB actionEffectDB)
    {
        s_actionEffectDB = actionEffectDB;
    }

    public static void ProvideGameRoot(AGameRoot gameRoot)
    {
        if (GodotObject.IsInstanceValid(s_gameRoot))
            s_gameRoot.Free();
        s_gameRoot = gameRoot;
    }

    public static void ProvideLoaderFactory(ILoaderFactory loaderFactory) => s_loaderFactory = loaderFactory;

    public static List<string> CheckReferences()
    {
        List<string> unsetRefs = new();
        if (s_actionEffectDB == null)
            unsetRefs.Add("ActionEffect DB");
        if (s_gameRoot == null)
            unsetRefs.Add("Game Root");
        if (s_loaderFactory == null)
            unsetRefs.Add("Loader Factory");
        return unsetRefs;
    }
}
