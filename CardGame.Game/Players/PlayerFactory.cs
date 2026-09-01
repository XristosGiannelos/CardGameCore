using CardGame.Domain.Cards;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;

namespace CardGame.Game.Players;

public static class PlayerFactory
{
    public static PlayerState Create(
        PlayerId playerId,
        LeaderState leader,
        IEnumerable<CardInstance> deck)
    {
        var player = new PlayerState
        {
            Id = playerId,
            Energy = 5,
            MaxEnergy = 5,
            Leader = leader
        };

        player.Deck.AddRange(deck);

        return player;
    }
}