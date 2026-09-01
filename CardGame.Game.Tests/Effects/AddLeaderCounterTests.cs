using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Effects;
using CardGame.Game.Game;

namespace CardGame.Tests.Effects;

public sealed class AddLeaderCounterEffectTests
{
    [Fact]
    public void Resolve_AddsCounter()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var context = CreateContext(
            playerOne,
            playerTwo);

        var effect = new AddLeaderCounterEffect(
            playerOne.Id,
            "Soul");

        effect.Resolve(context);

        Assert.Equal(
            1,
            playerOne.Leader.GetCounter("Soul"));
    }

    [Fact]
    public void Resolve_AddsRequestedAmount()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var context = CreateContext(
            playerOne,
            playerTwo);

        var effect = new AddLeaderCounterEffect(
            playerOne.Id,
            "Soul",
            3);

        effect.Resolve(context);

        Assert.Equal(
            3,
            playerOne.Leader.GetCounter("Soul"));
    }

    [Fact]
    public void Resolve_AddsToExistingCounter()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        playerOne.Leader.AddCounter(
            "Soul",
            4);

        var context = CreateContext(
            playerOne,
            playerTwo);

        var effect = new AddLeaderCounterEffect(
            playerOne.Id,
            "Soul",
            2);

        effect.Resolve(context);

        Assert.Equal(
            6,
            playerOne.Leader.GetCounter("Soul"));
    }

    [Fact]
    public void Resolve_ZeroAmount_DoesNothing()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        playerOne.Leader.AddCounter(
            "Soul",
            4);

        var context = CreateContext(
            playerOne,
            playerTwo);

        var effect = new AddLeaderCounterEffect(
            playerOne.Id,
            "Soul",
            0);

        effect.Resolve(context);

        Assert.Equal(
            4,
            playerOne.Leader.GetCounter("Soul"));
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
        PlayerState playerTwo)
    {
        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;
        game.TurnNumber = 1;
        game.Phase = GamePhase.Main;
        game.PriorityPlayerId = playerOne.Id;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        return new GameContext(
            game,
            registry);
    }
}