using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Game;

namespace CardGame.Tests.Game;

public sealed class StateBasedActionManagerTests
{
    [Fact]
    public void Check_UnitAtZeroHealth_MovesItToGraveyard()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var unit = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Battlefield,
            Damage = 1
        };

        playerOne.Battlefield.Add(unit);

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        var context = new GameContext(
            game,
            registry);

        var stateBasedActions =
            new StateBasedActionManager(context);

        stateBasedActions.Check();

        Assert.DoesNotContain(
            unit,
            playerOne.Battlefield);

        Assert.Contains(
            unit,
            playerOne.Graveyard);

        Assert.Equal(
            ZoneType.Graveyard,
            unit.Zone);

        Assert.Equal(
            0,
            unit.Damage);
    }

    [Fact]
    public void LeaderReachesZeroHealth_PlayerLoses()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        playerTwo.Leader.TakeDamage(
            playerTwo.Leader.CurrentHealth);

        var stateBasedActions =
            new StateBasedActionManager(context);

        stateBasedActions.Check();

        Assert.Equal(
            GameStatus.Finished,
            context.State.Status);

        Assert.Equal(
            playerTwo.Id,
            context.State.LoserId);

        Assert.Equal(
            playerOne.Id,
            context.State.WinnerId);
    }

    [Fact]
    public void LeaderAboveZeroHealth_GameContinues()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        playerTwo.Leader.TakeDamage(1);

        var stateBasedActions =
            new StateBasedActionManager(context);

        stateBasedActions.Check();

        Assert.Equal(
            GameStatus.InProgress,
            context.State.Status);

        Assert.Null(context.State.LoserId);
        Assert.Null(context.State.WinnerId);
    }

    private static PlayerState CreatePlayer()
    {
        var playerId = PlayerId.New();

        var leaderCard = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.Morga,
            OwnerId = playerId,
            ControllerId = playerId,
            Zone = ZoneType.Battlefield
        };

        return new PlayerState
        {
            Id = playerId,
            Energy = 5,
            MaxEnergy = 5,
            Leader = new LeaderState(
                leaderCard,
                30)
        };
    }

    private static GameContext CreateContext(
PlayerState playerOne,
PlayerState playerTwo,
PlayerId firstPlayerId)
    {
        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            firstPlayerId);

        game.Status = GameStatus.InProgress;
        game.TurnNumber = 1;
        game.Phase = GamePhase.Main;
        game.PriorityPlayerId = firstPlayerId;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        return new GameContext(
            game,
            registry);
    }

}