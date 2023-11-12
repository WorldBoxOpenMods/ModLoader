using NeoModLoader.constants;
using UnityEngine;

namespace NeoModLoader.api;

internal class AttachedModComponent : MonoBehaviour, IMod
{
    private ModDeclare _declare;
    public ModDeclare GetDeclaration()
    {
        return _declare;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
    public ModConfig GetConfig()
    {
        return null;
    }

    public string GetUrl()
    {
        return string.IsNullOrEmpty(_declare.RepoUrl) ? CoreConstants.OrgURL : _declare.RepoUrl;
    }

    public void OnLoad(ModDeclare pModDecl, GameObject pGameObject)
    {
        _declare = pModDecl;
    }
}