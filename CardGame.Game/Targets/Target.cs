using CardGame.Domain.Identifiers;

namespace CardGame.Game.Targets;

public abstract record Target
{
    private Target()
    {
    }

    public sealed record Unit(CardInstanceId InstanceId) : Target;

    public sealed record Leader(PlayerId PlayerId) : Target;
}