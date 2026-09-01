using CardGame.Domain.Identifiers;

namespace CardGame.Game.Events;

public sealed record CardDiscardedEvent(
    CardInstanceId CardInstanceId,
    PlayerId PlayerId) : GameEvent;