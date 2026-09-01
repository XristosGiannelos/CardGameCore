using CardGame.Domain.Identifiers;

namespace CardGame.Game.Events;

public sealed record UnitEnteredBattlefieldEvent(
    CardInstanceId CardInstanceId,
    PlayerId ControllerId) : GameEvent;