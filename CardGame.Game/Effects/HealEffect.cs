using CardGame.Domain.Game;
using CardGame.Game.Context;
using CardGame.Game.Targets;

namespace CardGame.Game.Effects;

public sealed class HealEffect : IEffect
{
    private readonly Target _target;
    private readonly int _amount;

    public HealEffect(
        Target target,
        int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        _target = target;
        _amount = amount;
    }

    public void Resolve(GameContext context)
    {
        switch (_target)
        {
            case Target.Unit unitTarget:
                {
                    var unit = context.FindCard(
                        unitTarget.InstanceId);

                    if (unit is null)
                        return;

                    if (unit.Zone != ZoneType.Battlefield)
                        return;

                    unit.Damage = Math.Max(
                        0,
                        unit.Damage - _amount);

                    break;
                }

            case Target.Leader leaderTarget:
                {
                    var leader =
                        context.GetPlayer(
                            leaderTarget.PlayerId).Leader;

                    leader.Heal(_amount);

                    break;
                }

            default:
                throw new InvalidOperationException(
                    $"Unsupported target type: {_target.GetType().Name}");
        }
    }
}