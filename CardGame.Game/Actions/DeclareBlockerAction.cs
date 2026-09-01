using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Game.Context;

namespace CardGame.Game.Actions;

public sealed class DeclareBlockerAction : IGameAction
{
    public PlayerId PlayerId { get; }

    public CardInstanceId AttackerId { get; }

    public CardInstanceId BlockerId { get; }

    public ActionSpeed Speed => ActionSpeed.Instant;

    public DeclareBlockerAction(
        PlayerId playerId,
        CardInstanceId attackerId,
        CardInstanceId blockerId)
    {
        PlayerId = playerId;
        AttackerId = attackerId;
        BlockerId = blockerId;
    }

    public bool CanExecute(GameContext context)
    {
        var attacker = context.FindCard(AttackerId);
        var blocker = context.FindCard(BlockerId);

        if (attacker is null || blocker is null)
            return false;

        if (attacker.Zone != ZoneType.Battlefield)
            return false;

        if (blocker.Zone != ZoneType.Battlefield)
            return false;

        if (blocker.ControllerId != PlayerId)
            return false;

        if (attacker.ControllerId == blocker.ControllerId)
            return false;

        if (context.Combat is null)
            return false;

        if (context.Combat.AttackerId != AttackerId)
            return false;

        if (context.Combat.BlockerId is not null)
            return false;

        if (blocker.ReadyState != CardReadyState.Untapped)
            return false;

        return true;
    }

    public void Execute(GameContext context)
    {
        if (!CanExecute(context))
        {
            throw new InvalidOperationException(
                "The blocker declaration is not legal.");
        }

        var blocker = context.FindCard(BlockerId)!;

        context.Combat!.SetBlocker(BlockerId);

        blocker.ReadyState = CardReadyState.Tapped;
    }
}