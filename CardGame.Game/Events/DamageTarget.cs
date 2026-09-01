using CardGame.Domain.Identifiers;

namespace CardGame.Game.Events;

public abstract record DamageTarget
{
    private DamageTarget()
    {
    }

    public sealed record Unit(CardInstanceId InstanceId)
        : DamageTarget;

    public sealed record Leader(PlayerId PlayerId)
        : DamageTarget;
}