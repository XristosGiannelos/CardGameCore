using CardGame.Domain.Identifiers;
using CardGame.Domain.Game;
using CardGame.Game.Context;

namespace CardGame.Game.Actions;

public sealed class ActionValidator
{
    private readonly GameContext _context;

    public ActionValidator(GameContext context)
    {
        _context = context;
    }

    public void ValidatePlayerHasPriority(PlayerId playerId)
    {
        if (_context.State.PriorityPlayerId != playerId)
        {
            throw new InvalidOperationException(
                "This player does not have priority.");
        }
    }

    public void ValidateMainPhase(PlayerId playerId)
    {
        if (_context.State.Phase != GamePhase.Main)
        {
            throw new InvalidOperationException(
                "This action can only be performed during the Main Phase.");
        }

        ValidatePlayerHasPriority(playerId);
    }

    public void ValidateInstantSpeed(PlayerId playerId)
    {
        ValidatePlayerHasPriority(playerId);
    }

    public void ValidateGameInProgress()
    {
        if (_context.State.Status != GameStatus.InProgress)
        {
            throw new InvalidOperationException(
                "The game is not in progress.");
        }
    }
}