using CardGame.Domain.Identifiers;
using CardGame.Game.Context;
using CardGame.Game.Effects;
using CardGame.Game.Targets;

namespace CardGame.Game.Stack;

public sealed class AttackStackItem : StackItem
{
    public CardInstanceId AttackerId { get; }

    public Target Target { get; }

    public AttackStackItem(
        PlayerId controllerId,
        CardInstanceId attackerId,
        Target target)
        : base(controllerId)
    {
        AttackerId = attackerId;
        Target = target;
    }

    public override void Resolve(GameContext context)
    {
        var attacker = context.FindCard(AttackerId);

        if (attacker is null) { 
            context.Combat = null;
            return;
        }

        switch (Target)
        {
            case Target.Leader leaderTarget:
                {
                    if (context.Combat?.BlockerId is CardInstanceId blockerId)
                    {
                        var blocker = context.FindCard(blockerId);

                        if (blocker is not null)
                        {
                            blocker.Damage += context.GetAttack(attacker);
                            attacker.Damage += context.GetAttack(blocker);

                            if (blocker.Damage >= context.GetMaxHealth(blocker))
                            {
                                new DestroyUnitEffect(blocker.InstanceId)
                                    .Resolve(context);
                            }

                            if (attacker.Damage >= context.GetMaxHealth(attacker))
                            {
                                new DestroyUnitEffect(attacker.InstanceId)
                                    .Resolve(context);
                            }

                            context.Combat = null;
                            return;
                        }
                    }

                    var leader = context
                        .GetPlayer(leaderTarget.PlayerId)
                        .Leader;

                    leader.TakeDamage(
                        context.GetAttack(attacker));

                    context.Combat = null;

                    break;
                }

            case Target.Unit unitTarget:
                {
                    var defender = context.FindCard(unitTarget.InstanceId);

                    if (defender is null) { 
                        context.Combat = null;
                        return;
                    }
                    defender.Damage += context.GetAttack(attacker);
                    attacker.Damage += context.GetAttack(defender);

                    break;
                }

            default:
                throw new InvalidOperationException(
                    $"Unsupported attack target: {Target.GetType().Name}");
        }
        context.Combat = null;
    }
}