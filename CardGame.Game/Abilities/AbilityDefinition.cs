using CardGame.Domain.Identifiers;
using CardGame.Game.Context;
using CardGame.Game.Effects;
using CardGame.Game.Events;

namespace CardGame.Game.Abilities;

public sealed class AbilityDefinition
{
    public string Name { get; }

    public ITrigger Trigger { get; }

    private readonly Func<GameContext, GameEvent, CardInstanceId, IReadOnlyList<IEffect>> _effectFactory;

    public AbilityDefinition(
        string name,
        ITrigger trigger,
        Func<GameContext, GameEvent, CardInstanceId, IReadOnlyList<IEffect>> effectFactory)
    {
        Name = name;
        Trigger = trigger;
        _effectFactory = effectFactory;
    }

    public bool CanTrigger(
        GameContext context,
        GameEvent gameEvent,
        CardInstanceId sourceInstanceId)
    {
        return Trigger.Matches(
            context,
            gameEvent,
            sourceInstanceId);
    }

    public IReadOnlyList<IEffect> CreateEffects(
        GameContext context,
        GameEvent gameEvent,
        CardInstanceId sourceInstanceId)
    {
        return _effectFactory(
            context,
            gameEvent,
            sourceInstanceId);
    }
}