using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;

public sealed class GameState
{
    public GameId Id { get; init; }

    public PlayerState PlayerOne { get; init; } = null!;

    public PlayerState PlayerTwo { get; init; } = null!;

    public PlayerId FirstPlayerId { get; init; }

    public PlayerId ActivePlayerId { get; set; }

    public PlayerId PriorityPlayerId { get; set; }

    public int TurnNumber { get; set; }

    public GamePhase Phase { get; set; }

    public GameStatus Status { get; set; }
    public PlayerId? WinnerId { get; set; }
    public PlayerId? LoserId { get; set; }
    public int ConsecutivePasses { get; set; }

    public bool IsFirstTurn => TurnNumber == 1;

    public PlayerState GetPlayer(PlayerId playerId)
    {
        if (PlayerOne.Id == playerId)
            return PlayerOne;

        if (PlayerTwo.Id == playerId)
            return PlayerTwo;

        throw new InvalidOperationException(
            $"Player {playerId} does not belong to this game.");
    }

    public PlayerState OpponentOf(PlayerId playerId)
    {
        if (PlayerOne.Id == playerId)
            return PlayerTwo;

        if (PlayerTwo.Id == playerId)
            return PlayerOne;

        throw new InvalidOperationException(
            $"Player {playerId} does not belong to this game.");
    }
}