using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Effects;
using CardGame.Game.Game;

namespace CardGame.Tests.Effects;

public sealed class GainEnergyEffectTests
{
    [Fact]
    public void Resolve_GainsEnergy()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        playerOne.Energy = 2;

        var context = CreateContext(
            playerOne,
            playerTwo);

        var effect = new GainEnergyEffect(
            playerOne.Id,
            2);

        effect.Resolve(context);

        Assert.Equal(4, playerOne.Energy);
    }

    [Fact]
    public void Resolve_DoesNotExceedMaxEnergy()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        playerOne.Energy = 4;

        var context = CreateContext(
            playerOne,
            playerTwo);

        var effect = new GainEnergyEffect(
            playerOne.Id,
            2);

        effect.Resolve(context);

        Assert.Equal(
            playerOne.MaxEnergy,
            playerOne.Energy);
    }

    [Fact]
    public void Resolve_ZeroAmount_DoesNothing()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        playerOne.Energy = 3;

        var context = CreateContext(
            playerOne,
            playerTwo);

        var effect = new GainEnergyEffect(
            playerOne.Id,
            0);

        effect.Resolve(context);

        Assert.Equal(3, playerOne.Energy);
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