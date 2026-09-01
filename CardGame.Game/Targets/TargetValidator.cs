using CardGame.Domain.Game;
using CardGame.Game.Context;

namespace CardGame.Game.Targets;

public sealed class TargetValidator
{
    private readonly GameContext _context;

    public TargetValidator(GameContext context)
    {
        _context = context;
    }

    public bool IsValid(Target target)
    {
        switch (target)
        {
            case Target.Unit unitTarget:
                {
                    var unit = _context.FindCard(
                        unitTarget.InstanceId);

                    if (unit is null)
                        return false;

                    return unit.Zone == ZoneType.Battlefield;
                }

            case Target.Leader leaderTarget:
                {
                    try
                    {
                        _context.GetPlayer(
                            leaderTarget.PlayerId);

                        return true;
                    }
                    catch (InvalidOperationException)
                    {
                        return false;
                    }
                }

            default:
                return false;
        }
    }
}