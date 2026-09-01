using CardGame.Domain.Identifiers;
using CardGame.Game.Context;

namespace CardGame.Game.Actions;

public sealed class PassPriorityAction : IGameAction
{
    public PlayerId PlayerId { get; }
    public ActionSpeed Speed => ActionSpeed.Instant;
    public PassPriorityAction(PlayerId playerId)
    {
        PlayerId = playerId;
    }

    public bool CanExecute(GameContext context)
    {
        return context.State.PriorityPlayerId == PlayerId;
    }

    public void Execute(GameContext context)
    {
        throw new InvalidOperationException(
            "PassPriorityAction must be processed by GameActionExecutor.");
    }
}