namespace CardGame.Domain.Identifiers;

public readonly record struct CardId(string Value)
{
    public override string ToString() => Value;
}