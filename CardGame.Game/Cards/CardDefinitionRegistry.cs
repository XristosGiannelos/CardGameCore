using CardGame.Domain.Cards;
using CardGame.Domain.Identifiers;

namespace CardGame.Game.Cards;

public sealed class CardDefinitionRegistry
{
    private readonly Dictionary<CardId, CardDefinition> _definitions = [];

    public void Register(CardDefinition definition)
    {
        _definitions[definition.Id] = definition;
    }

    public CardDefinition Get(CardId cardId)
    {
        if (!_definitions.TryGetValue(cardId, out var definition))
        {
            throw new InvalidOperationException(
                $"Card definition '{cardId}' was not found.");
        }

        return definition;
    }

    public bool TryGet(
        CardId cardId,
        out CardDefinition? definition)
    {
        return _definitions.TryGetValue(cardId, out definition);
    }
}