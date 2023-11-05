using UnityEngine;

namespace NeoModLoader.api;

public interface IMod
{
    public ModDeclare GetDeclaration();
    public GameObject GetGameObject();
    public string GetUrl();
    public void OnLoad(ModDeclare pModDecl, GameObject pGameObject);
}