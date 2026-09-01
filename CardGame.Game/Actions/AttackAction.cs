using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Game.Combat;
using CardGame.Game.Context;
using CardGame.Game.Stack;
using CardGame.Game.Targets;

namespace CardGame.Game.Actions;

public sealed class AttackAction : IGameAction
{
    public PlayerId PlayerId { get; }

    public CardInstanceId AttackerId { get; }

    public Target Target { get; }

    public ActionSpeed Speed => ActionSpeed.Main;

    public AttackAction(
        PlayerId playerId,
        CardInstanceId attackerId,
        Target target)
    {
        PlayerId = playerId;
        AttackerId = attackerId;
        Target = target;
    }

    public bool CanExecute(GameContext context)
    {
        if (context.State.Phase != GamePhase.Main)
            return false;

        var player = context.GetPlayer(PlayerId);

        var attacker = player.Battlefield
            .FirstOrDefault(x => x.InstanceId == AttackerId);

        if (attacker is null)
            return false;

        if (attacker.Zone != ZoneType.Battlefield)
            return false;

        if (attacker.ReadyState != CardReadyState.Untapped)
            return false;

        if (context.State.IsFirstTurn &&
            context.State.FirstPlayerId == PlayerId)
            return false;

        return TargetIsLegal(context, player, attacker);
    }

    public void Execute(GameContext context)
    {
        var player = context.GetPlayer(PlayerId);

        var attacker = player.Battlefield
            .First(x => x.InstanceId == AttackerId);

        attacker.ReadyState = CardReadyState.Tapped;

        context.Combat = new CombatState(
    AttackerId,
    Target);

        var attack = new AttackStackItem(
    PlayerId,
    AttackerId,
    Target);

        context.Stack.Push(attack);
    }

    private bool TargetIsLegal(
        GameContext context,
        Domain.Players.PlayerState player,
        CardInstance attacker)
    {
        switch (Target)
        {
            case Target.Leader leaderTarget:
                return leaderTarget.PlayerId != PlayerId;

            case Target.Unit unitTarget:
                {
                    var target = context.FindCard(unitTarget.InstanceId);

                    if (target is null)
                        return false;

                    if (target.Zone != ZoneType.Battlefield)
                        return false;

                    var opponent = context.GetOpponent(PlayerId);

                    if (!opponent.Battlefield
                        .Any(x => x.InstanceId == target.InstanceId))
                        return false;

                    return target.ReadyState == CardReadyState.Tapped;
                }

            default:
                return false;
        }
    }
}