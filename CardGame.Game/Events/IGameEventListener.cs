namespace CardGame.Game.Events;

public interface IGameEventListener
{
    void Handle(GameEvent gameEvent);
}