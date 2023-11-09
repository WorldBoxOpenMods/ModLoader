namespace NeoModLoader.utils.authentication;

internal static class DiscordCommonAuthLogic
{
    internal static IEnumerable<string> GetRolesOfUser(string user_id)
    {
        HttpResponseMessage res = HttpUtils.Get("https://65.108.87.77:3000/user/roles/" + user_id, new Dictionary<string, string>());
        // expected response [ a list of strings ], empty if user has no roles, not on server, or ID doesn't exist
        string resJson = res.Content.ReadAsStringAsync().Result;
        return resJson.Trim('[', ']', ' ').Split(',').Select(role => role.Trim('"', ' '));
    }
    
    internal static bool ModderIsInRolesList(IEnumerable<string> roles)
    {
        return roles.Any(role => role == "647734005625651220");
    }
}