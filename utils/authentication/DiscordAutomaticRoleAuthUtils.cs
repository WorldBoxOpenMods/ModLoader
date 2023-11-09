namespace NeoModLoader.utils.authentication;

public class DiscordAutomaticRoleAuthUtils
{
    public static bool Authenticate()
    {
        if (Config.gameLoaded)
        {
            if (typeof(Config).GetField("discordId")?.GetValue(null) is string user_id)
            {
                return DiscordCommonAuthLogic.ModderIsInRolesList(DiscordCommonAuthLogic.GetRolesOfUser(user_id));
            }
            throw new AuthenticaticationException("The game was unable to fetch a Discord ID.");
        }
        throw new AuthenticaticationException("The game isn't loaded yet, so no Discord ID is available.");
    }
}