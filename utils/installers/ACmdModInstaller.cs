namespace NeoModLoader.utils.installers;

internal abstract class ACmdModInstaller
{
    public abstract Task<bool> CheckInstall(string pParam);
}