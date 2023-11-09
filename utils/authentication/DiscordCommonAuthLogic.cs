namespace NeoModLoader.utils.authentication;

internal static class DiscordCommonAuthLogic
{
    internal static IEnumerable<string> GetRolesOfUser(string user_id)
    {
        HttpResponseMessage res = HttpUtils.Get("URL_OF_JUPE_API" + user_id, new Dictionary<string, string>());
        // expected response [ a list of strings ], empty if user has no roles, not on server, or ID doesn't exist
        return Array.Empty<string>();
    }
    
    internal static bool ModderIsInRolesList(IEnumerable<string> roles)
    {
        return roles.Any(role => role == "647734005625651220");
    }
}