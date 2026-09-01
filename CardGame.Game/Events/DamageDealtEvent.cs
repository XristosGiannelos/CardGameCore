

namespace CardGame.Game.Events;

public sealed record DamageDealtEvent(
    int Amount,
    DamageTarget Target) : GameEvent;