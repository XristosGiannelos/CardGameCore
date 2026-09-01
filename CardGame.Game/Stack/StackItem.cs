using CardGame.Domain.Identifiers;
using CardGame.Game.Context;

namespace CardGame.Game.Stack;

public abstract class StackItem
{
    public StackItemId Id { get; } = StackItemId.New();

    public PlayerId ControllerId { get; }

    protected StackItem(PlayerId controllerId)
    {
        ControllerId = controllerId;
    }

    public abstract void Resolve(GameContext context);
}