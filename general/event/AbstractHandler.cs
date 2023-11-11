namespace NeoModLoader.General.Event;

public abstract class AbstractHandler<THandler> where THandler : AbstractHandler<THandler>
{
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