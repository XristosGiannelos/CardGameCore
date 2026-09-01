using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game;
using CardGame.Game.Actions;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Game;

namespace CardGame.Tests.Game;

public sealed class PlayUnitActionTests
{
    [Fact]
    public void PlayUnit_FromHand_PaysEnergyAndMovesToBattlefield()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var unit = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Hand
        };

        playerOne.Hand.Add(unit);

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;
        game.TurnNumber = 2;
        game.Phase = GamePhase.Main;
        game.PriorityPlayerId = playerOne.Id;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        var context = new GameContext(
            game,
            registry);

        var engine = new GameEngine(context);

        var startingEnergy = playerOne.Energy;

        var action = new PlayCardAction(
            playerOne.Id,
            unit.InstanceId);

        Assert.True(
            action.CanExecute(context));

        engine.ExecuteAction(action);

        Assert.DoesNotContain(
            unit,
            playerOne.Hand);

        Assert.Contains(
            unit,
            playerOne.Battlefield);

        Assert.Equal(
            ZoneType.Battlefield,
            unit.Zone);

        Assert.Equal(
            startingEnergy - 1,
            playerOne.Energy);
    }

    [Fact]
    public void CannotPlay_CardOwnedByOpponent()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var unit = new CardInstance
            {
                InstanceId = CardInstanceId.New(),
                DefinitionId = CardIds.GraveRat,
                OwnerId = playerTwo.Id,
                ControllerId = playerTwo.Id,
                Zone = ZoneType.Hand
            };

            playerTwo.Hand.Add(unit);

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            var action = new PlayCardAction(
                playerOne.Id,
                unit.InstanceId);

            Assert.False(
                action.CanExecute(context));
    }

    [Fact]
    public void CannotPlay_UnitAlreadyOnBattlefield()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var unit = new CardInstance
            {
                InstanceId = CardInstanceId.New(),
                DefinitionId = CardIds.GraveRat,
                OwnerId = playerOne.Id,
                ControllerId = playerOne.Id,
                Zone = ZoneType.Battlefield
            };

            playerOne.Battlefield.Add(unit);

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            var action = new PlayCardAction(
                playerOne.Id,
                unit.InstanceId);

            Assert.False(
                action.CanExecute(context));
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
        game.TurnNumber = 2;
        game.Phase = GamePhase.Main;
        game.PriorityPlayerId = firstPlayerId;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        return new GameContext(
            game,
            registry);
    }


}

