using NeoModLoader.constants;
using UnityEngine;

namespace NeoModLoader.api;

/// <summary>
///     This class is made for ncms mod to get <see cref="ModDeclare" /> for themselves
/// </summary>
public class AttachedModComponent : MonoBehaviour, IMod
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

    public string GetUrl()
    {
        return string.IsNullOrEmpty(_declare.RepoUrl) ? CoreConstants.OrgURL : _declare.RepoUrl;
    }

    public void OnLoad(ModDeclare pModDecl, GameObject pGameObject)
    {
        _declare = pModDecl;
    }
}