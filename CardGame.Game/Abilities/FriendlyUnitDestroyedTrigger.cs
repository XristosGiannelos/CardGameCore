using CardGame.Domain.Identifiers;
using CardGame.Game.Context;
using CardGame.Game.Events;

namespace CardGame.Game.Abilities;

public sealed class FriendlyUnitDestroyedTrigger : ITrigger
{
    private readonly bool _includeSource;

    public FriendlyUnitDestroyedTrigger(bool includeSource = true)
    {
        _includeSource = includeSource;
    }

    public bool Matches(
        GameContext context,
        GameEvent gameEvent,
        CardInstanceId sourceInstanceId)
    {
        if (gameEvent is not UnitDestroyedEvent destroyedEvent)
            return false;

        var source = context.FindCard(sourceInstanceId);

        if (source is null)
            return false;

        if (!_includeSource &&
            destroyedEvent.CardInstanceId == sourceInstanceId)
        {
            return false;
        }

        return destroyedEvent.OwnerId == source.OwnerId;
    }
}