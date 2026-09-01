using CardGame.Domain.Identifiers;
using CardGame.Game.Context;
using CardGame.Game.Events;

namespace CardGame.Game.Abilities;

public sealed class SpellCastTrigger : ITrigger
{
    public bool Matches(
        GameContext context,
        GameEvent gameEvent,
        CardInstanceId sourceInstanceId)
    {
        if (gameEvent is not SpellCastEvent spellEvent)
            return false;

        var source = context.FindCard(sourceInstanceId);

        if (source is null)
            return false;

        return spellEvent.ControllerId == source.OwnerId;
    }
}