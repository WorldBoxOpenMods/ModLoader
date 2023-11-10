namespace NeoModLoader.General.Event;

public abstract class AbstractHandler<HandlerType> where HandlerType : AbstractHandler<HandlerType>
{
    protected static List<HandlerType> _handlers = new();
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
    public static void Register(HandlerType handler)
    {
        _handlers.Add(handler);
    }
}