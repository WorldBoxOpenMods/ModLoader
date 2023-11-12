using NeoModLoader.constants;
using UnityEngine;

namespace NeoModLoader.api;
/// <summary>
/// This class is used to represent a mod that is not loaded by NeoModLoader.
/// </summary>
public class VirtualMod : IMod
{
    private ModDeclare _declare;
    private GameObject _bindedGameObject;
    public ModDeclare GetDeclaration()
    {
        return _declare;
    }

    public GameObject GetGameObject()
    {
        return _bindedGameObject;
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
        // TODO: Find pGameObject and set it.
        _declare = pModDecl;
        _bindedGameObject = pGameObject;
    }
}