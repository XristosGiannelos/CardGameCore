using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Game.Context;

namespace CardGame.Game.Game;

public sealed class GameEndManager
{
    private readonly GameContext _context;

    public GameEndManager(GameContext context)
    {
        _context = context;
    }

    public void PlayerLoses(PlayerId loserId)
    {
        var game = _context.State;

        if (game.Status == GameStatus.Finished)
            return;

        var winner = game.OpponentOf(loserId);

        game.Status = GameStatus.Finished;
        game.LoserId = loserId;
        game.WinnerId = winner.Id;
    }
}