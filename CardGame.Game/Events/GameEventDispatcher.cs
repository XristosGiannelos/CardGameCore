namespace CardGame.Game.Events;

public sealed class GameEventDispatcher
{
    private readonly List<IGameEventListener> _listeners = [];

    public void Subscribe(IGameEventListener listener)
    {
        if (!_listeners.Contains(listener))
            _listeners.Add(listener);
    }

    public void Unsubscribe(IGameEventListener listener)
    {
        _listeners.Remove(listener);
    }

    public void Dispatch(GameEvent gameEvent)
    {
        foreach (var listener in _listeners.ToArray())
        {
            listener.Handle(gameEvent);
        }
    }
}