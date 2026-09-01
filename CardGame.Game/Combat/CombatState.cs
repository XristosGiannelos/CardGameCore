using CardGame.Domain.Identifiers;
using CardGame.Game.Targets;

namespace CardGame.Game.Combat;

public sealed class CombatState
{
    public CardInstanceId AttackerId { get; }

    public Target Target { get; }

    public CardInstanceId? BlockerId { get; private set; }

    public CombatState(
        CardInstanceId attackerId,
        Target target)
    {
        AttackerId = attackerId;
        Target = target;
    }

    public void SetBlocker(CardInstanceId blockerId)
    {
        if (BlockerId is not null)
        {
            throw new InvalidOperationException(
                "This attack already has a blocker.");
        }

        BlockerId = blockerId;
    }
}