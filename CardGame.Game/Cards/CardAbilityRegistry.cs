using CardGame.Domain.Identifiers;
using CardGame.Game.Abilities;

namespace CardGame.Game.Cards;

public sealed class CardAbilityRegistry
{
    private readonly Dictionary<CardId, IReadOnlyList<AbilityDefinition>> _abilities = [];

    public void Register(
        CardId cardId,
        params AbilityDefinition[] abilities)
    {
        _abilities[cardId] = abilities;
    }

    public IReadOnlyList<AbilityDefinition> GetAbilities(CardId cardId)
    {
        return _abilities.TryGetValue(cardId, out var abilities)
            ? abilities
            : [];
    }
}