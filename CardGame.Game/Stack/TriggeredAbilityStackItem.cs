using CardGame.Domain.Identifiers;
using CardGame.Game.Context;
using CardGame.Game.Effects;

namespace CardGame.Game.Stack;

public sealed class TriggeredAbilityStackItem : StackItem
{
    public CardInstanceId SourceInstanceId { get; }

    public string AbilityName { get; }

    private readonly IReadOnlyList<IEffect> _effects;

    public TriggeredAbilityStackItem(
        PlayerId controllerId,
        CardInstanceId sourceInstanceId,
        string abilityName,
        IReadOnlyList<IEffect> effects)
        : base(controllerId)
    {
        SourceInstanceId = sourceInstanceId;
        AbilityName = abilityName;
        _effects = effects;
    }

    public override void Resolve(GameContext context)
    {
        foreach (var effect in _effects)
        {
            effect.Resolve(context);
        }
    }
}