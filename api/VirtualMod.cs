using NeoModLoader.constants;
using UnityEngine;

namespace NeoModLoader.api;
/// <summary>
/// This class is used to represent a mod that is not loaded by NeoModLoader.
/// </summary>
public class VirtualMod : IMod
{
    private ModDeclare _declare;
    private GameObject _boundGameObject;
    ///<inheritdoc />
    public ModDeclare GetDeclaration()
    {
        return _declare;
    }
    ///<inheritdoc />
    public GameObject GetGameObject()
    {
        return _boundGameObject;
    }
    ///<inheritdoc />
    public string GetUrl()
    {
        return string.IsNullOrEmpty(_declare.RepoUrl) ? CoreConstants.OrgURL : _declare.RepoUrl;
    }
    ///<inheritdoc />
    public void OnLoad(ModDeclare pModDecl, GameObject pGameObject)
    {
        _declare = pModDecl;
        _boundGameObject = pGameObject;
    }
}