using CardGame.Domain.Identifiers;
using CardGame.Game.Context;
using CardGame.Game.Stack;

namespace CardGame.Game.Turn;

public sealed class PriorityManager
{
    private readonly GameContext _context;

    public PriorityManager(GameContext context)
    {
        _context = context;
    }

    public PlayerId CurrentPlayer =>
        _context.State.PriorityPlayerId;

    public void PassPriority()
    {
        var state = _context.State;

        var opponent = state.OpponentOf(state.PriorityPlayerId);

        state.PriorityPlayerId = opponent.Id;

        state.ConsecutivePasses++;
    }

    public bool BothPlayersPassed()
    {
        return _context.State.ConsecutivePasses >= 2;
    }

    public void ResetAfterAction(PlayerId actionPlayer)
    {
        _context.State.ConsecutivePasses = 0;

        SetPriority(actionPlayer);
    }
    public void ResetAfterResolution()
    {
        _context.State.ConsecutivePasses = 0;

        _context.State.PriorityPlayerId =
            _context.State.ActivePlayerId;
    }
    private void SetPriority(PlayerId playerId)
    {
        _context.State.PriorityPlayerId = playerId;
    }
    public void GivePriorityToOpponent(PlayerId playerId)
    {
        var opponent = _context.State.OpponentOf(playerId);

        _context.State.PriorityPlayerId = opponent.Id;
        _context.State.ConsecutivePasses = 0;
    }

}