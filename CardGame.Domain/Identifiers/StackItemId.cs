namespace CardGame.Domain.Identifiers;

public readonly record struct StackItemId(Guid Value)
{
    public static StackItemId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}