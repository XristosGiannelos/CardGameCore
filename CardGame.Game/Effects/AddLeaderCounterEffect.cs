using CardGame.Domain.Identifiers;
using CardGame.Game.Context;

namespace CardGame.Game.Effects;

public sealed class AddLeaderCounterEffect : IEffect
{
    private readonly PlayerId _playerId;
    private readonly string _counterName;
    private readonly int _amount;

    public AddLeaderCounterEffect(
        PlayerId playerId,
        string counterName,
        int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(counterName))
            throw new ArgumentException(
                "Counter name cannot be empty.",
                nameof(counterName));

        if (amount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(amount));

        _playerId = playerId;
        _counterName = counterName;
        _amount = amount;
    }

    public void Resolve(GameContext context)
    {
        var leader =
            context.GetPlayer(_playerId).Leader;

        leader.AddCounter(
            _counterName,
            _amount);
    }
}