using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;

namespace CardGame.Domain.Cards;

public sealed class CardInstance
{
    public CardInstanceId InstanceId { get; init; }

    public CardId DefinitionId { get; init; }

    public PlayerId OwnerId { get; init; }

    public PlayerId ControllerId { get; set; }

    public ZoneType Zone { get; set; }

    public int Damage { get; set; }

    public int AttackModifier { get; set; }

    public int HealthModifier { get; set; }
    public CardReadyState ReadyState { get; set; } = CardReadyState.Untapped;
}