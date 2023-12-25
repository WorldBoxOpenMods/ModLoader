namespace NeoModLoader.General.Event;
/// <summary>
/// Abstract handler for event
/// </summary>
/// <typeparam name="THandler"></typeparam>
public abstract class AbstractHandler<THandler> where THandler : AbstractHandler<THandler>
{
    /// <summary>
    /// Wheather this handler is enabled. It is disabled when error_hit reach 10
    /// </summary>
    public bool enabled { get; private set; } = true;
    private int error_hit = 0;
    internal void HitException()
    {
        error_hit++;
        if(error_hit > 10)
        {
            enabled = false;
        }
    }
}