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
    ///<inheritdoc />
    public ModDeclare GetDeclaration()
    {
        return _declare;
    }
    ///<inheritdoc />
    public GameObject GetGameObject()
    {
        return _bindedGameObject;
    }
    ///<inheritdoc />
    public string GetUrl()
    {
        return string.IsNullOrEmpty(_declare.RepoUrl) ? CoreConstants.OrgURL : _declare.RepoUrl;
    }
    ///<inheritdoc />
    public void OnLoad(ModDeclare pModDecl, GameObject pGameObject)
    {
        // TODO: Find pGameObject and set it.
        _declare = pModDecl;
        _bindedGameObject = pGameObject;
    }
}