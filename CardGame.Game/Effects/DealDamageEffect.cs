using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Game.Context;
using CardGame.Game.Targets;

namespace CardGame.Game.Effects;

public sealed class DealDamageEffect : IEffect
{
    private readonly Target _target;
    private readonly int _amount;

    public DealDamageEffect(Target target, int amount)
    {
        _target = target;
        _amount = amount;
    }

    public void Resolve(GameContext context)
    {
        switch (_target)
        {
            case Target.Unit unitTarget:
                {
                    var unit = context.FindCard(unitTarget.InstanceId);

                    if (unit is null)
                        return;

                    if (unit.Zone != ZoneType.Battlefield)
                        return;

                    unit.Damage += _amount;

                    break;
                }

            case Target.Leader leaderTarget:
                {
                    var leader = context.GetPlayer(leaderTarget.PlayerId).Leader;

                    leader.TakeDamage(_amount);

                    break;
                }

            default:
                throw new InvalidOperationException(
                    $"Unsupported target type: {_target.GetType().Name}");
        }
    }
}