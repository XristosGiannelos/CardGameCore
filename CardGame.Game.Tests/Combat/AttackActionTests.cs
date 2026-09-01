using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game;
using CardGame.Game.Actions;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Game;
using CardGame.Game.Targets;
using CardGame.Game.Turn;

namespace CardGame.Tests.Combat;

public sealed class AttackActionTests
{
    [Fact]
    public void FirstPlayer_CannotAttack_OnFirstTurn()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var attacker = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Battlefield
        };

        playerOne.Battlefield.Add(attacker);

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;
        game.Phase = GamePhase.Main;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        var context = new GameContext(
            game,
            registry);

        var action = new AttackAction(
            playerOne.Id,
            attacker.InstanceId,
            new Target.Leader(playerTwo.Id));

        Assert.False(action.CanExecute(context));
    }

    [Fact]
    public void StartTurn_FirstPlayerOnFirstTurn_DoesNotDraw()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var card = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Deck
        };

        playerOne.Deck.Add(card);

        var initialHandSize = playerOne.Hand.Count;
        var initialDeckSize = playerOne.Deck.Count;

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;
        game.TurnNumber = 1;
        game.ActivePlayerId = playerOne.Id;
        game.PriorityPlayerId = playerOne.Id;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        var context = new GameContext(
            game,
            registry);

        var turnManager = new TurnManager(context);

        turnManager.StartTurn();

        Assert.Equal(
            initialHandSize,
            playerOne.Hand.Count);

        Assert.Equal(
            initialDeckSize,
            playerOne.Deck.Count);

        Assert.Contains(
            card,
            playerOne.Deck);

        Assert.Equal(
            ZoneType.Deck,
            card.Zone);
    }

    [Fact]
    public void StartTurn_SecondPlayerFirstTurn_DrawsOneCard()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var card = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerTwo.Id,
            ControllerId = playerTwo.Id,
            Zone = ZoneType.Deck
        };

        playerTwo.Deck.Add(card);

        var initialHandSize = playerTwo.Hand.Count;

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;

        // First player's turn has already happened.
        game.TurnNumber = 2;

        // Now it is Player Two's turn.
        game.ActivePlayerId = playerTwo.Id;
        game.PriorityPlayerId = playerTwo.Id;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        var context = new GameContext(
            game,
            registry);

        var turnManager = new TurnManager(context);

        turnManager.StartTurn();

        Assert.Equal(
            initialHandSize + 1,
            playerTwo.Hand.Count);

        Assert.DoesNotContain(
            card,
            playerTwo.Deck);

        Assert.Contains(
            card,
            playerTwo.Hand);

        Assert.Equal(
            ZoneType.Hand,
            card.Zone);
    }

    [Fact]
    public void StartTurn_NormalTurn_DrawsOneCard()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var card = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Deck
        };

        playerOne.Deck.Add(card);

        var initialHandSize = playerOne.Hand.Count;

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;
        game.TurnNumber = 2;
        game.ActivePlayerId = playerOne.Id;
        game.PriorityPlayerId = playerOne.Id;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        var context = new GameContext(
            game,
            registry);

        var turnManager = new TurnManager(context);

        turnManager.StartTurn();

        Assert.Equal(
            initialHandSize + 1,
            playerOne.Hand.Count);

        Assert.DoesNotContain(
            card,
            playerOne.Deck);

        Assert.Contains(
            card,
            playerOne.Hand);

        Assert.Equal(
            ZoneType.Hand,
            card.Zone);
    }

    [Fact]
    public void Attack_ResolvesThroughPriorityLoop_AndDealsDamageToLeader()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var attacker = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Battlefield
        };

        playerOne.Battlefield.Add(attacker);

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;
        game.TurnNumber = 2;
        game.Phase = GamePhase.Main;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        var context = new GameContext(
            game,
            registry);

        var engine = new GameEngine(context);

        var startingHealth = playerTwo.Leader.CurrentHealth;

        var attack = new AttackAction(
            playerOne.Id,
            attacker.InstanceId,
            new Target.Leader(playerTwo.Id));

        // Player One declares the attack.
        engine.ExecuteAction(attack);

        Assert.True(
            attacker.ReadyState == CardReadyState.Tapped);

        Assert.False(
            context.Stack.IsEmpty);

        Assert.Equal(
            playerTwo.Id,
            context.State.PriorityPlayerId);

        // Player Two passes priority.
        engine.ExecuteAction(
            new PassPriorityAction(playerTwo.Id));

        Assert.Equal(
            playerOne.Id,
            context.State.PriorityPlayerId);

        Assert.Equal(
            1,
            context.State.ConsecutivePasses);

        // Player One passes priority.
        engine.ExecuteAction(
            new PassPriorityAction(playerOne.Id));

        // Both players passed, so the attack should resolve.
        Assert.True(
            context.Stack.IsEmpty);

        Assert.Equal(
            startingHealth - 2,
            playerTwo.Leader.CurrentHealth);

        Assert.Equal(
            0,
            context.State.ConsecutivePasses);
    }

    [Fact]
    public void Attack_UnitVsUnit_BothUnitsDieAndMoveToGraveyard()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var attacker = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Battlefield
        };

        var defender = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerTwo.Id,
            ControllerId = playerTwo.Id,
            Zone = ZoneType.Battlefield,
            ReadyState = CardReadyState.Tapped
        };

        playerOne.Battlefield.Add(attacker);
        playerTwo.Battlefield.Add(defender);

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;
        game.TurnNumber = 2;
        game.Phase = GamePhase.Main;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        var context = new GameContext(
            game,
            registry);

        var engine = new GameEngine(context);

        var attack = new AttackAction(
            playerOne.Id,
            attacker.InstanceId,
            new Target.Unit(defender.InstanceId));

        Assert.True(attack.CanExecute(context));

        engine.ExecuteAction(attack);

        // Player Two passes.
        engine.ExecuteAction(
            new PassPriorityAction(playerTwo.Id));

        // Player One passes.
        engine.ExecuteAction(
            new PassPriorityAction(playerOne.Id));

        // Both units should have died.
        Assert.DoesNotContain(
            attacker,
            playerOne.Battlefield);

        Assert.DoesNotContain(
            defender,
            playerTwo.Battlefield);

        // Both should now be in their owner's graveyard.
        Assert.Contains(
            attacker,
            playerOne.Graveyard);

        Assert.Contains(
            defender,
            playerTwo.Graveyard);

        Assert.Equal(
            ZoneType.Graveyard,
            attacker.Zone);

        Assert.Equal(
            ZoneType.Graveyard,
            defender.Zone);

        Assert.Equal(0, attacker.Damage);
        Assert.Equal(0, defender.Damage);

        Assert.True(context.Stack.IsEmpty);
    }

    [Fact]
    public void Attack_UnitVsUnit_DefenderSurvivesAndAttackerDies()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var attacker = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Battlefield
        };

        var defender = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerTwo.Id,
            ControllerId = playerTwo.Id,
            Zone = ZoneType.Battlefield,
            ReadyState = CardReadyState.Tapped,
            HealthModifier = 4
        };

        playerOne.Battlefield.Add(attacker);
        playerTwo.Battlefield.Add(defender);

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;
        game.TurnNumber = 2;
        game.Phase = GamePhase.Main;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        var context = new GameContext(
            game,
            registry);

        var engine = new GameEngine(context);

        var attack = new AttackAction(
            playerOne.Id,
            attacker.InstanceId,
            new Target.Unit(defender.InstanceId));

        Assert.True(attack.CanExecute(context));

        engine.ExecuteAction(attack);

        // Player Two passes.
        engine.ExecuteAction(
            new PassPriorityAction(playerTwo.Id));

        // Player One passes.
        engine.ExecuteAction(
            new PassPriorityAction(playerOne.Id));

        // Attacker should be dead.
        Assert.DoesNotContain(
            attacker,
            playerOne.Battlefield);

        Assert.Contains(
            attacker,
            playerOne.Graveyard);

        Assert.Equal(
            ZoneType.Graveyard,
            attacker.Zone);

        // Defender should survive.
        Assert.Contains(
            defender,
            playerTwo.Battlefield);

        Assert.DoesNotContain(
            defender,
            playerTwo.Graveyard);

        Assert.Equal(
            ZoneType.Battlefield,
            defender.Zone);

        Assert.Equal(
            2,
            defender.Damage);

        Assert.Equal(
            0,
            attacker.Damage);

        Assert.True(
            context.Stack.IsEmpty);
    }

    [Fact]
    public void Attack_UnitVsUnit_AttackerSurvivesAndDefenderDies()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var attacker = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Battlefield,
            HealthModifier = 1
        };

        var defender = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerTwo.Id,
            ControllerId = playerTwo.Id,
            Zone = ZoneType.Battlefield,
            ReadyState = CardReadyState.Tapped,
            AttackModifier = -1
        };

        playerOne.Battlefield.Add(attacker);
        playerTwo.Battlefield.Add(defender);

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;
        game.TurnNumber = 2;
        game.Phase = GamePhase.Main;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        var context = new GameContext(
            game,
            registry);

        var engine = new GameEngine(context);

        var attack = new AttackAction(
            playerOne.Id,
            attacker.InstanceId,
            new Target.Unit(defender.InstanceId));

        Assert.True(attack.CanExecute(context));

        engine.ExecuteAction(attack);

        // Player Two passes.
        engine.ExecuteAction(
            new PassPriorityAction(playerTwo.Id));

        // Player One passes.
        engine.ExecuteAction(
            new PassPriorityAction(playerOne.Id));

        // Defender should be dead.
        Assert.DoesNotContain(
            defender,
            playerTwo.Battlefield);

        Assert.Contains(
            defender,
            playerTwo.Graveyard);

        Assert.Equal(
            ZoneType.Graveyard,
            defender.Zone);

        Assert.Equal(
            0,
            defender.Damage);

        // Attacker should survive.
        Assert.Contains(
            attacker,
            playerOne.Battlefield);

        Assert.DoesNotContain(
            attacker,
            playerOne.Graveyard);

        Assert.Equal(
            ZoneType.Battlefield,
            attacker.Zone);

        // Attacker took 1 damage.
        Assert.Equal(
            1,
            attacker.Damage);

        Assert.True(
            context.Stack.IsEmpty);
    }

    [Fact]
    public void Attack_TappedAttacker_CannotAttackAgain()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var attacker = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Battlefield
        };

        playerOne.Battlefield.Add(attacker);

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;
        game.TurnNumber = 2;
        game.Phase = GamePhase.Main;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        var context = new GameContext(
            game,
            registry);

        var engine = new GameEngine(context);

        var firstAttack = new AttackAction(
            playerOne.Id,
            attacker.InstanceId,
            new Target.Leader(playerTwo.Id));

        Assert.True(firstAttack.CanExecute(context));

        engine.ExecuteAction(firstAttack);

        Assert.Equal(
            CardReadyState.Tapped,
            attacker.ReadyState);

        var secondAttack = new AttackAction(
            playerOne.Id,
            attacker.InstanceId,
            new Target.Leader(playerTwo.Id));

        Assert.False(secondAttack.CanExecute(context));
    }

    [Fact]
    public void StartTurn_UntapsPlayerBattlefield()
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
            ReadyState = CardReadyState.Tapped
        };

        playerOne.Battlefield.Add(unit);

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;
        game.TurnNumber = 2;
        game.ActivePlayerId = playerOne.Id;
        game.PriorityPlayerId = playerOne.Id;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        var context = new GameContext(
            game,
            registry);

        var turnManager = new TurnManager(context);

        Assert.Equal(
            CardReadyState.Tapped,
            unit.ReadyState);

        turnManager.StartTurn();

        Assert.Equal(
            CardReadyState.Untapped,
            unit.ReadyState);
    }

    [Fact]
    public void Attack_LeaderReachesZeroHealth_GameEnds()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var attacker = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Battlefield
        };

        playerOne.Battlefield.Add(attacker);

        // Make the attack lethal.
        playerTwo.Leader.TakeDamage(
            playerTwo.Leader.CurrentHealth - 2);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var engine = new GameEngine(context);

        var attack = new AttackAction(
            playerOne.Id,
            attacker.InstanceId,
            new Target.Leader(playerTwo.Id));

        Assert.True(
            attack.CanExecute(context));

        engine.ExecuteAction(attack);

        // Resolve combat.
        engine.ExecuteAction(
            new PassPriorityAction(playerTwo.Id));

        engine.ExecuteAction(
            new PassPriorityAction(playerOne.Id));

        Assert.Equal(
            GameStatus.Finished,
            context.State.Status);

        Assert.Equal(
            playerTwo.Id,
            context.State.LoserId);

        Assert.Equal(
            playerOne.Id,
            context.State.WinnerId);

        Assert.True(
            context.Stack.IsEmpty);
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