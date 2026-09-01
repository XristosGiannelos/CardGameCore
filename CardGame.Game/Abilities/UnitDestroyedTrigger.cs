using CardGame.Domain.Identifiers;
using CardGame.Game.Context;
using CardGame.Game.Events;

namespace CardGame.Game.Abilities;

public sealed class UnitDestroyedTrigger : ITrigger
{
    private readonly bool _onlySource;

    public UnitDestroyedTrigger(bool onlySource = false)
    {
        _onlySource = onlySource;
    }

    public bool Matches(
        GameContext context,
        GameEvent gameEvent,
        CardInstanceId sourceInstanceId)
    {
        if (gameEvent is not UnitDestroyedEvent destroyedEvent)
            return false;

        if (!_onlySource)
            return true;

        return destroyedEvent.CardInstanceId == sourceInstanceId;
    }
}