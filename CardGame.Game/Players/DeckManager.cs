using CardGame.Domain.Players;
using CardGame.Domain.Game;
using CardGame.Game.Context;

namespace CardGame.Game.Players;

public sealed class DeckManager
{
    private readonly Random _random;
    public DeckManager(Random? random = null)
    {
        _random = random ?? Random.Shared;
    }

    public void Shuffle(PlayerState player)
    {
        for (var i = player.Deck.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);

            (player.Deck[i], player.Deck[j]) =
                (player.Deck[j], player.Deck[i]);
        }
    }

    public void Draw(PlayerState player, int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            if (player.Deck.Count == 0)
                return;

            var card = player.Deck[0];

            player.Deck.RemoveAt(0);
            player.Hand.Add(card);

            card.Zone = ZoneType.Hand;
        }
    }
}