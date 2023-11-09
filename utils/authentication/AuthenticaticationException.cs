namespace NeoModLoader.utils.authentication;

public class AuthenticaticationException : Exception
{
    public AuthenticaticationException()
    {
    }
    public AuthenticaticationException(string message) : base(message)
    {
    }
    public AuthenticaticationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}