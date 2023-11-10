namespace NeoModLoader.General.Event;

public abstract class AbstractHandler<HandlerType> where HandlerType : AbstractHandler<HandlerType>
{
    protected static List<HandlerType> _handlers = new();

    internal static void HandleAll(params object[] @params)
    {
        foreach (HandlerType handler in _handlers)
            handler.Handle(@params);
    }
    public static void Register(HandlerType handler)
    {
        _handlers.Add(handler);
    }
    protected abstract void Handle(object[] @params);
}