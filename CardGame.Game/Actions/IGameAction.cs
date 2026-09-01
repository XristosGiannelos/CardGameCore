using CardGame.Domain.Identifiers;
using CardGame.Game.Context;

namespace CardGame.Game.Actions;

public interface IGameAction
{
    PlayerId PlayerId { get; }
    ActionSpeed Speed { get; }
    bool CanExecute(GameContext context);
    void Execute(GameContext context);
}