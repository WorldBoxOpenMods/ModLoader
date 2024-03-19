using HarmonyLib;
using NeoModLoader.constants;

namespace NeoModLoader.General.Game.extensions;

/// <summary>
///     Extension helper for asset libraries
/// </summary>
public static class AssetExtension
{
    /// <summary>
    ///     Do actions on every assets in library <see cref="pLibrary" /> including assets in the future
    /// </summary>
    /// <param name="pLibrary"></param>
    /// <param name="pAction"></param>
    /// <typeparam name="TAsset"></typeparam>
    /// <typeparam name="TLibrary"></typeparam>
    public static void ForEach<TAsset, TLibrary>(this TLibrary pLibrary, Action<TAsset> pAction)
        where TAsset : Asset
        where TLibrary : AssetLibrary<TAsset>
    {
        AssetExtensionInternal<TAsset, TLibrary>.ForEach(pLibrary, pAction);
    }
}

internal static class AssetExtensionInternal<TAsset, TLibrary>
    where TAsset : Asset
    where TLibrary : AssetLibrary<TAsset>
{
    private static readonly Dictionary<TLibrary, List<LibraryState>> _states = new();
    private static          bool                                     _assetlibrary_patched;

    public static void ForEach(TLibrary pLibrary, Action<TAsset> pAction)
    {
        if (pLibrary == null) return;

        var state = new LibraryState();

        foreach (TAsset asset in pLibrary.list) pAction(asset);

        state.action = asset => { pAction(asset); };
        state.done.UnionWith(pLibrary.list.Select(x => x.id));

        if (!_states.ContainsKey(pLibrary)) _states.Add(pLibrary, new List<LibraryState>());
        _states[pLibrary].Add(state);

        if (_assetlibrary_patched) return;
        _assetlibrary_patched = true;
        new Harmony($"{CoreConstants.ModName}.ForEach").Patch(
            AccessTools.Method(typeof(AssetLibrary<TAsset>), nameof(AssetLibrary<TAsset>.add)),
            postfix: new HarmonyMethod(
                AccessTools.FirstMethod(typeof(AssetExtensionInternal<TAsset, TLibrary>),
                                        x => x.Name.Contains(nameof(AppendAssetToAction)))));
    }

    private static void AppendAssetToAction(TLibrary __instance, TAsset pAsset)
    {
        if (!_states.TryGetValue(__instance, out var states)) return;
        foreach (LibraryState state in states)
        {
            if (!state.done.Add(pAsset.id)) return;
            state.action(pAsset);
        }
    }

    private class LibraryState
    {
        public readonly HashSet<string> done = new();
        public          Action<TAsset>  action;
    }
}