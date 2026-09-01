using CardGame.Domain.Identifiers;

namespace CardGame.Game.Events;

public sealed record SpellCastEvent(
    CardInstanceId CardInstanceId,
    PlayerId ControllerId,
    bool IsToken) : GameEvent;