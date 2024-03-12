using NeoModLoader.api;
using NeoModLoader.General.UI.Prefabs;
using UnityEngine;

namespace NeoModLoader.ui.prefabs;

/// <summary>
///     Panel of displaying mod's information.
/// </summary>
public class ModInfoPanel : APrefab<ModInfoPanel>
{
    internal void Setup(ModDeclare pModDeclaration)
    {
        ModState mod_state = WorldBoxMod.AllRecognizedMods[pModDeclaration];
        if (mod_state == ModState.LOADED)
        {
            IMod mod = WorldBoxMod.LoadedMods.Find(x => x.GetDeclaration() == pModDeclaration);
            if (mod is IDecoratePanel decorate) decorate.DecoratePanel(this);
        }
    }

    private static void _init()
    {
        var obj = new GameObject("ModInfoPanel", typeof(RectTransform));


        Prefab = obj.AddComponent<ModInfoPanel>();
    }
}