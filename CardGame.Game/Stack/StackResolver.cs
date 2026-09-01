using CardGame.Game.Context;

namespace CardGame.Game.Stack;

public sealed class StackResolver
{
    private readonly GameContext _context;

    public StackResolver(GameContext context)
    {
        _context = context;
    }

    public bool TryResolveTop()
    {
        if (_context.Stack.IsEmpty)
            return false;

        var item = _context.Stack.Pop();

        item.Resolve(_context);

        return true;
    }
}