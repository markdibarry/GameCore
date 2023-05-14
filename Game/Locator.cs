using System;
using GameCore.ActionEffects;
using GameCore.Audio;
using GameCore.GUI;
using Godot;

namespace GameCore;

public static class Locator
{
    private static bool s_initialized;
    private static BaseGameRoot s_gameRoot = null!;
    private static ILoaderFactory? s_loaderFactory;
    private static readonly NullLoaderFactory s_nullLoaderFactory = new();
    private static IActionEffectDB? s_actionEffectDB;
    private static readonly NullActionEffectDB s_nullActionEffectDB = new();

    public static bool Initialized => s_initialized;
    public static IActionEffectDB ActionEffectDB => s_actionEffectDB ?? s_nullActionEffectDB;
    public static BaseAudioController Audio => s_gameRoot.AudioController;
    public static ILoaderFactory LoaderFactory => s_loaderFactory ?? s_nullLoaderFactory;
    public static BaseGameRoot Root => s_gameRoot;
    public static BaseGameSession? Session => s_gameRoot?.GameSession;
    public static BaseTransitionController TransitionController => s_gameRoot.TransitionController;

    public static void SetInitialized() => s_initialized = true;

    public static void ProvideActionEffectDB(IActionEffectDB actionEffectDB)
    {
        s_actionEffectDB = actionEffectDB;
    }

    public static void ProvideGameRoot(BaseGameRoot gameRoot)
    {
        if (GodotObject.IsInstanceValid(s_gameRoot))
            s_gameRoot.Free();
        s_gameRoot = gameRoot;
    }

    public static void ProvideLoaderFactory(ILoaderFactory loaderFactory) => s_loaderFactory = loaderFactory;

    private class NullActionEffectDB : IActionEffectDB
    {
        public IActionEffect? GetEffect(int type) => null;
    }

    private class NullLoaderFactory : ILoaderFactory
    {
        public ObjectLoader GetLoader(string path, Action reportCallback)
        {
            return new GUI.ResourceLoader(path, reportCallback);
        }
    }
}
