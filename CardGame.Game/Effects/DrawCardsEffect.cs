using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Game.Context;

namespace CardGame.Game.Effects;

public sealed class DrawCardsEffect : IEffect
{
    private readonly PlayerId _playerId;
    private readonly int _amount;

    public DrawCardsEffect(PlayerId playerId, int amount)
    {
        _playerId = playerId;
        _amount = amount;
    }

    public void Resolve(GameContext context)
    {
        var player = context.GetPlayer(_playerId);

        for (var i = 0; i < _amount; i++)
        {
            if (player.Deck.Count == 0)
            {
                context.PlayerLoses(_playerId);

                return;
            }

            var card = player.Deck[0];

            player.Deck.RemoveAt(0);
            player.Hand.Add(card);

            card.Zone = ZoneType.Hand;
        }
    }
}