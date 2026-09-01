namespace CardGame.Game.Stack;

public sealed class GameStack
{
    private readonly List<StackItem> _items = [];

    public int Count => _items.Count;

    public bool IsEmpty => _items.Count == 0;

    public IReadOnlyList<StackItem> Items => _items;

    public void Push(StackItem item)
    {
        _items.Add(item);
    }

    public StackItem Pop()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("The stack is empty.");

        var index = _items.Count - 1;

        var item = _items[index];

        _items.RemoveAt(index);

        return item;
    }

    public StackItem Peek()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("The stack is empty.");

        return _items[^1];
    }
}