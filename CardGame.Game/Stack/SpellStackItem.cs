using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Game.Context;
using CardGame.Game.Effects;
using CardGame.Game.Targets;
namespace CardGame.Game.Stack;

public sealed class SpellStackItem : StackItem
{
    public CardInstance Card { get; }

    public Target? Target { get; }

    private readonly IReadOnlyList<IEffect> _effects;

    public SpellStackItem(
        PlayerId controllerId,
        CardInstance card,
        Target? target,
        IReadOnlyList<IEffect> effects)
        : base(controllerId)
    {
        Card = card;
        Target = target;
        _effects = effects;
    }

    public override void Resolve(GameContext context)
    {
        foreach (var effect in _effects)
        {
            effect.Resolve(context);
        }

        var owner = context.GetPlayer(Card.OwnerId);

        if (owner.Graveyard.Contains(Card))
            return;

        owner.Graveyard.Add(Card);

        Card.Zone = ZoneType.Graveyard;
    }
}