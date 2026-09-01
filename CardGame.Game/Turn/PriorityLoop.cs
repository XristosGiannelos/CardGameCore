using CardGame.Game.Context;
using CardGame.Game.Game;
using CardGame.Game.Stack;

namespace CardGame.Game.Turn;

public sealed class PriorityLoop
{
    private readonly GameContext _context;
    private readonly PriorityManager _priority;
    private readonly StackResolver _stackResolver;
    private readonly StateBasedActionManager _stateBasedActions;

    public PriorityLoop(
        GameContext context,
        PriorityManager priority,
        StackResolver stackResolver,
        StateBasedActionManager stateBasedActions)
    {
        _context = context;
        _priority = priority;
        _stackResolver = stackResolver;
        _stateBasedActions = stateBasedActions;
    }

    public void ResolveIfBothPlayersPassed()
    {
        if (!_priority.BothPlayersPassed())
            return;

        if (_context.Stack.IsEmpty)
        {
            _context.State.ConsecutivePasses = 0;
            return;
        }

        _stackResolver.TryResolveTop();

        _stateBasedActions.Check();

        _priority.ResetAfterResolution();
    }
}