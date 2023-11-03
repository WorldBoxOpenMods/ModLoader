using NeoModLoader.services;
using UnityEngine;

namespace NeoModLoader.api;

public abstract class BasicMod : MonoBehaviour, IMod
{
    private ModDeclare _declare = null!;
    public GameObject GetGameObject()
    {
        return gameObject;
    }


    public virtual void OnLoad(ModDeclare pModDecl, GameObject pGameObject)
    {
        _declare = pModDecl;
        LogInfo("OnLoad");
    }
    public void LogInfo(string message)
    {
        LogService.LogInfo($"[{_declare.Name}]: {message}");
    }
    public void LogWarning(string message)
    {
        LogService.LogWarning($"[{_declare.Name}]: {message}");
    }
    public void LogError(string message)
    {
        LogService.LogError($"[{_declare.Name}]: {message}");
    }

    public ModDeclare GetDeclaration()
    {
        return _declare;
    }
}