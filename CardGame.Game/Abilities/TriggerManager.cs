using CardGame.Domain.Cards;
using CardGame.Game.Context;
using CardGame.Game.Events;
using CardGame.Game.Stack;

namespace CardGame.Game.Abilities;

public sealed class TriggerManager : IGameEventListener
{
    private readonly GameContext _context;

    public TriggerManager(GameContext context)
    {
        _context = context;
    }

    public void Handle(GameEvent gameEvent)
    {
        foreach (var card in GetCardsInPlay())
        {
            var abilities =
                _context.CardAbilities.GetAbilities(card.DefinitionId);

            foreach (var ability in abilities)
            {
                if (!ability.CanTrigger(
                        _context,
                        gameEvent,
                        card.InstanceId))
                {
                    continue;
                }

                var effects = ability.CreateEffects(
                    _context,
                    gameEvent,
                    card.InstanceId);

                var stackItem = new TriggeredAbilityStackItem(
                    card.ControllerId,
                    card.InstanceId,
                    ability.Name,
                    effects);

                _context.Stack.Push(stackItem);
            }
        }
    }

    private IEnumerable<CardInstance> GetCardsInPlay()
    {
        foreach (var card in _context.State.PlayerOne.Battlefield)
            yield return card;

        foreach (var card in _context.State.PlayerTwo.Battlefield)
            yield return card;
    }
}