using CardGame.Domain.Cards;
using CardGame.Game.Context;
using CardGame.Game.Turn;

namespace CardGame.Game.Actions;

public sealed class GameActionExecutor
{
    private readonly GameContext _context;
    private readonly PriorityManager _priority;
    private readonly ActionValidator _validator;

    public GameActionExecutor(
        GameContext context,
        PriorityManager priority)
    {
        _context = context;
        _priority = priority;
        _validator = new ActionValidator(context);
    }

    public void Execute(IGameAction action)
    {
        _validator.ValidateGameInProgress();

        if (!action.CanExecute(_context))
        {
            throw new InvalidOperationException(
                "The action is not legal.");
        }

        if (action is PassPriorityAction)
        {
            _priority.PassPriority();
            return;
        }

        if (action is PlayCardAction playCard)
        {
            var card = _context.FindCard(playCard.CardInstanceId);

            if (card is null)
            {
                throw new InvalidOperationException(
                    "Card does not exist.");
            }

            var definition =
                _context.CardDefinitions.Get(card.DefinitionId);

            if (definition.Type == CardType.Spell)
            {
                _validator.ValidateInstantSpeed(action.PlayerId);
            }
            else
            {
                _validator.ValidateMainPhase(action.PlayerId);
            }
        }
        else if (action.Speed == ActionSpeed.Main)
        {
            _validator.ValidateMainPhase(action.PlayerId);
        }
        else
        {
            _validator.ValidateInstantSpeed(action.PlayerId);
        }

        action.Execute(_context);

        if (action is EndTurnAction)
            return;

        if (action is AttackAction)
        {
            _priority.GivePriorityToOpponent(action.PlayerId);
            return;
        }

        _priority.ResetAfterAction(action.PlayerId);
    }
}