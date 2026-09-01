using CardGame.Domain.Identifiers;

namespace CardGame.Game.Events;

public sealed record UnitDestroyedEvent(
    CardInstanceId CardInstanceId,
    PlayerId OwnerId) : GameEvent;