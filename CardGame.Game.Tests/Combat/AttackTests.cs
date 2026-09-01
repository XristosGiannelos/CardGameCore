using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game.Actions;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Game;
using CardGame.Game.Stack;
using CardGame.Game.Targets;

namespace CardGame.Tests.Combat;

public sealed class AttackTests
{
    [Fact]
    public void AttackUnit_DealsDamageToDefender()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var attacker = CreateUnit(playerOne.Id);
        var defender = CreateUnit(playerTwo.Id);

        playerOne.Battlefield.Add(attacker);
        playerTwo.Battlefield.Add(defender);

        defender.ReadyState = CardReadyState.Tapped;

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new AttackAction(
            playerOne.Id,
            attacker.InstanceId,
            new Target.Unit(defender.InstanceId));

        Assert.True(action.CanExecute(context));

        action.Execute(context);

        var stackItem =
            Assert.IsType<AttackStackItem>(
                context.Stack.Peek());

        stackItem.Resolve(context);

        // Both are Grave Rats: 2 ATK / 1 HP.
        Assert.Equal(2, defender.Damage);
        Assert.Equal(2, attacker.Damage);
    }

    [Fact]
    public void AttackLeader_DealsDamageToLeader()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var attacker = CreateUnit(
            playerOne.Id,
            attack: 2,
            health: 3);

        playerOne.Battlefield.Add(attacker);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var startingHealth =
            playerTwo.Leader.CurrentHealth;

        var action = new AttackAction(
            playerOne.Id,
            attacker.InstanceId,
            new Target.Leader(playerTwo.Id));

        Assert.True(action.CanExecute(context));

        action.Execute(context);

        var stackItem =
            Assert.IsType<AttackStackItem>(
                context.Stack.Peek());

        stackItem.Resolve(context);

        Assert.Equal(
            startingHealth - 2,
            playerTwo.Leader.CurrentHealth);
    }

    [Fact]
    public void Attack_LeavesAttackerTapped()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var attacker = CreateUnit(
            playerOne.Id,
            attack: 2,
            health: 3);

        playerOne.Battlefield.Add(attacker);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new AttackAction(
            playerOne.Id,
            attacker.InstanceId,
            new Target.Leader(playerTwo.Id));

        action.Execute(context);

        Assert.Equal(
            CardReadyState.Tapped,
            attacker.ReadyState);
    }

    [Fact]
    public void Attack_LeavesCombatAfterResolution()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var attacker = CreateUnit(
            playerOne.Id,
            attack: 2,
            health: 3);

        playerOne.Battlefield.Add(attacker);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new AttackAction(
            playerOne.Id,
            attacker.InstanceId,
            new Target.Leader(playerTwo.Id));

        action.Execute(context);

        Assert.NotNull(context.Combat);

        var stackItem =
            Assert.IsType<AttackStackItem>(
                context.Stack.Peek());

        stackItem.Resolve(context);

        Assert.Null(context.Combat);
    }

    private static CardInstance CreateUnit(
        PlayerId ownerId,
        int attack,
        int health)
    {
        return new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = ownerId,
            ControllerId = ownerId,
            Zone = ZoneType.Battlefield
        };
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
        game.ActivePlayerId = firstPlayerId;
        game.PriorityPlayerId = firstPlayerId;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        return new GameContext(
            game,
            registry);
    }
    private static CardInstance CreateUnit(PlayerId ownerId)
    {
        return new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = ownerId,
            ControllerId = ownerId,
            Zone = ZoneType.Battlefield
        };
    }
}