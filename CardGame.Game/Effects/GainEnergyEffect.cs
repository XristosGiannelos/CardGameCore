using CardGame.Domain.Identifiers;
using CardGame.Game.Context;

namespace CardGame.Game.Effects;

public sealed class GainEnergyEffect : IEffect
{
    private readonly PlayerId _playerId;
    private readonly int _amount;

    public GainEnergyEffect(
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

        player.Energy = Math.Min(
            player.MaxEnergy,
            player.Energy + _amount);
    }
}