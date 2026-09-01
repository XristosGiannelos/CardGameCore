using CardGame.Domain.Identifiers;
using CardGame.Game.Context;
using CardGame.Game.Effects;
using CardGame.Game.Targets;

namespace CardGame.Game.Cards;

public sealed class CardEffectRegistry
{
    private readonly Dictionary<
        CardId,
        Func<GameContext, CardInstanceId, Target?, IReadOnlyList<IEffect>>> _effects = [];

    public void Register(
        CardId cardId,
        Func<GameContext, CardInstanceId, Target?, IReadOnlyList<IEffect>> effectFactory)
    {
        _effects[cardId] = effectFactory;
    }

    public IReadOnlyList<IEffect> CreateEffects(
        CardId cardId,
        GameContext context,
        CardInstanceId cardInstanceId,
        Target? target)
    {
        if (!_effects.TryGetValue(cardId, out var factory))
            return [];

        return factory(context, cardInstanceId,target);
    }
}