using CardGame.Domain.Cards;
using CardGame.Domain.Identifiers;

namespace CardGame.Domain.Players;

public sealed class PlayerState
{
    public PlayerId Id { get; init; }

    public int Energy { get; set; }

    public int MaxEnergy { get; set; } = 5;

    public LeaderState Leader { get; set; } = null!;

    public List<CardInstance> Deck { get; } = [];

    public List<CardInstance> Hand { get; } = [];

    public List<CardInstance> Battlefield { get; } = [];

    public List<CardInstance> Graveyard { get; } = [];

    public List<CardInstance> Banish { get; } = [];

    public int SpellsCastThisTurn { get; set; }
}