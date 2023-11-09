namespace NeoModLoader.utils.authentication;

internal static class DiscordCommonAuthLogic
{
    internal static IEnumerable<string> GetRolesOfUser(string user_id)
    {
        return Array.Empty<string>();
    }
    
    internal static bool ModderIsInRolesList(IEnumerable<string> roles)
    {
        return roles.Any(role => role == "647734005625651220");
    }
}