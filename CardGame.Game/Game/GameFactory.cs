using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;

namespace CardGame.Game.Game;

public static class GameFactory
{
    public static GameState Create(
        PlayerState playerOne,
        PlayerState playerTwo,
        PlayerId firstPlayerId)
    {
        var gameId = GameId.New();

        return new GameState
        {
            Id = gameId,

            PlayerOne = playerOne,
            PlayerTwo = playerTwo,

            FirstPlayerId = firstPlayerId,
            ActivePlayerId = firstPlayerId,
            PriorityPlayerId = firstPlayerId,

            TurnNumber = 1,
            Phase = GamePhase.Beginning,

            Status = GameStatus.NotStarted
        };
    }
}