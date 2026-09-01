using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Game.Context;

namespace CardGame.Game.Actions;

public sealed class EndTurnAction : IGameAction
{
    public PlayerId PlayerId { get; }

    public ActionSpeed Speed => ActionSpeed.Main;

    public EndTurnAction(PlayerId playerId)
    {
        PlayerId = playerId;
    }

    public bool CanExecute(GameContext context)
    {
        if (context.State.Status != GameStatus.InProgress)
            return false;

        if (context.State.Phase != GamePhase.Main)
            return false;

        if (context.State.ActivePlayerId != PlayerId)
            return false;

        if (!context.Stack.IsEmpty)
            return false;

        return context.State.PriorityPlayerId == PlayerId;
    }

    public void Execute(GameContext context)
    {
        if (!CanExecute(context))
        {
            throw new InvalidOperationException(
                "The turn cannot be ended.");
        }
    }
}
