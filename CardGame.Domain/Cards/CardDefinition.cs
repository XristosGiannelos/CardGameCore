using CardGame.Domain.Identifiers;

namespace CardGame.Domain.Cards;

public sealed class CardDefinition
{
    public CardId Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public CardType Type { get; init; }

    public CardColor Color { get; init; }

    public int EnergyCost { get; init; }

    public int? BaseAttack { get; init; }

    public int? BaseHealth { get; init; }

}