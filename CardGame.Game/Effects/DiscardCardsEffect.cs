using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Game.Context;

namespace CardGame.Game.Effects;

public sealed class DiscardCardsEffect : IEffect
{
    private readonly PlayerId _playerId;
    private readonly int _amount;

    public DiscardCardsEffect(
        PlayerId playerId,
        int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        _playerId = playerId;
        _amount = amount;
    }

    public void Resolve(GameContext context)
    {
        var player = context.GetPlayer(_playerId);

        if (player.Hand.Count < _amount)
            throw new InvalidOperationException(
                "The player does not have enough cards to discard.");

        for (var i = 0; i < _amount; i++)
        {
            var card = player.Hand[^1];

            player.Hand.RemoveAt(player.Hand.Count - 1);

            card.Zone = ZoneType.Graveyard;

            player.Graveyard.Add(card);
        }
    }
}