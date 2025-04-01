namespace NeoModLoader.utils.authentication;

public class DiscordAutomaticRoleAuthUtils
{
    public static bool Authenticate()
    {
        if (Config.game_loaded)
        {
            if (Config.discordId != null)
            {
                return DiscordCommonAuthLogic.ModderIsInRolesList(DiscordCommonAuthLogic.GetRolesOfUser(Config.discordId));
            }
            throw new AuthenticaticationException("The game was unable to fetch a Discord ID.");
        }
        throw new AuthenticaticationException("The game isn't loaded yet, so no Discord ID is available.");
    }
}