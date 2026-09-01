using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Effects;
using CardGame.Game.Game;
using CardGame.Game.Targets;

namespace CardGame.Tests.Effects;

public sealed class HealEffectTests
{
    [Fact]
    public void Resolve_HealsUnit_ByRemovingDamage()
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

        var context = CreateContext(
            playerOne,
            playerTwo);

        var effect = new HealEffect(
            new Target.Unit(unit.InstanceId),
            1);

        effect.Resolve(context);

        Assert.Equal(0, unit.Damage);
    }

    [Fact]
    public void Resolve_DoesNotHealUnit_AboveFullHealth()
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
            Damage = 0
        };

        playerOne.Battlefield.Add(unit);

        var context = CreateContext(
            playerOne,
            playerTwo);

        var effect = new HealEffect(
            new Target.Unit(unit.InstanceId),
            5);

        effect.Resolve(context);

        Assert.Equal(0, unit.Damage);
    }

    [Fact]
    public void Resolve_HealsLeader()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var context = CreateContext(
            playerOne,
            playerTwo);

        playerOne.Leader.TakeDamage(10);

        var effect = new HealEffect(
            new Target.Leader(playerOne.Id),
            3);

        effect.Resolve(context);

        Assert.Equal(
            23,
            playerOne.Leader.CurrentHealth);
    }

    [Fact]
    public void Resolve_DoesNotHealLeader_AboveMaximum()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var context = CreateContext(
            playerOne,
            playerTwo);

        playerOne.Leader.TakeDamage(2);

        var effect = new HealEffect(
            new Target.Leader(playerOne.Id),
            10);

        effect.Resolve(context);

        Assert.Equal(
            playerOne.Leader.MaxHealth,
            playerOne.Leader.CurrentHealth);
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