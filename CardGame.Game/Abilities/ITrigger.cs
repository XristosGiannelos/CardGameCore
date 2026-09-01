using CardGame.Domain.Identifiers;
using CardGame.Game.Context;
using CardGame.Game.Events;

namespace CardGame.Game.Abilities;

public interface ITrigger
{
    bool Matches(
        GameContext context,
        GameEvent gameEvent,
        CardInstanceId sourceInstanceId);
}