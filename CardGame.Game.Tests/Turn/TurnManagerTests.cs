using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Effects;
using CardGame.Game.Game;
using CardGame.Game.Players;
using CardGame.Game.Turn;

namespace CardGame.Tests.Turn;
public sealed class TurnManagerTests
{

    [Fact]
    public void StartTurn_RefreshesEnergy_UntapsUnits_AndResetsCounters()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        playerOne.Energy = 1;
        playerOne.SpellsCastThisTurn = 3;

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

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var turnManager = new TurnManager(context);

        turnManager.StartTurn();

        Assert.Equal(
            playerOne.MaxEnergy,
            playerOne.Energy);

        Assert.Equal(
            0,
            playerOne.SpellsCastThisTurn);

        Assert.Equal(
            CardReadyState.Untapped,
            unit.ReadyState);

        Assert.Equal(
            GamePhase.Main,
            context.State.Phase);

        Assert.Equal(
            playerOne.Id,
            context.State.PriorityPlayerId);
    }

    [Fact]
    public void StartTurn_WithEmptyDeck_PlayerLoses()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        // Make this a normal draw turn.
        context.State.TurnNumber = 2;

        var turnManager = new TurnManager(context);

        turnManager.StartTurn();

        Assert.Equal(
            GameStatus.Finished,
            context.State.Status);

        Assert.Equal(
            playerOne.Id,
            context.State.LoserId);

        Assert.Equal(
            playerTwo.Id,
            context.State.WinnerId);
    }

    [Fact]
    public void DrawCardsEffect_EmptyDeck_PlayerLoses()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var effect = new DrawCardsEffect(
            playerOne.Id,
            1);

        effect.Resolve(context);

        Assert.Equal(
            GameStatus.Finished,
            context.State.Status);

        Assert.Equal(
            playerOne.Id,
            context.State.LoserId);

        Assert.Equal(
            playerTwo.Id,
            context.State.WinnerId);
    }

    [Fact]
    public void StartTurn_FirstPlayer_DoesNotDrawOnFirstTurn()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        playerOne.Deck.Add(
            new CardInstance
            {
                InstanceId = CardInstanceId.New(),
                DefinitionId = CardIds.GraveRat,
                OwnerId = playerOne.Id,
                ControllerId = playerOne.Id,
                Zone = ZoneType.Deck
            });

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        context.State.TurnNumber = 1;
        context.State.ActivePlayerId = playerOne.Id;

        var startingHandSize = playerOne.Hand.Count;

        var turnManager = new TurnManager(context);

        turnManager.StartTurn();

        Assert.Equal(
            startingHandSize,
            playerOne.Hand.Count);
    }

    [Fact]
    public void StartTurn_DrawsOneCard_ForNormalTurn()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        playerOne.Deck.Add(
            new CardInstance
            {
                InstanceId = CardInstanceId.New(),
                DefinitionId = CardIds.GraveRat,
                OwnerId = playerOne.Id,
                ControllerId = playerOne.Id,
                Zone = ZoneType.Deck
            });

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        context.State.TurnNumber = 2;

        var startingHandSize = playerOne.Hand.Count;

        var turnManager = new TurnManager(context);

        turnManager.StartTurn();

        Assert.Equal(
            startingHandSize + 1,
            playerOne.Hand.Count);
    }


    [Fact]
    public void EndTurn_SwitchesActivePlayer()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var turnManager = new TurnManager(context);

        turnManager.EndTurn();

        Assert.Equal(
            playerTwo.Id,
            context.State.ActivePlayerId);

        Assert.Equal(
            playerTwo.Id,
            context.State.PriorityPlayerId);

        Assert.Equal(
            2,
            context.State.TurnNumber);

        Assert.Equal(
            0,
            context.State.ConsecutivePasses);
    }

    [Fact]
    public void EndTurn_ClearsDamageFromAllUnits()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var unitOne = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Battlefield,
            Damage = 2
        };

        var unitTwo = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerTwo.Id,
            ControllerId = playerTwo.Id,
            Zone = ZoneType.Battlefield,
            Damage = 3
        };

        playerOne.Battlefield.Add(unitOne);
        playerTwo.Battlefield.Add(unitTwo);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        context.State.TurnNumber = 2;

        var turnManager = new TurnManager(context);

        turnManager.EndTurn();

        Assert.Equal(0, unitOne.Damage);
        Assert.Equal(0, unitTwo.Damage);
    }

    [Fact]
    public void EndTurn_DoesNotClearCardModifiers()
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
            Damage = 2,
            AttackModifier = 3,
            HealthModifier = 4
        };

        playerOne.Battlefield.Add(unit);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        context.State.TurnNumber = 2;

        var turnManager = new TurnManager(context);

        turnManager.EndTurn();

        Assert.Equal(0, unit.Damage);
        Assert.Equal(3, unit.AttackModifier);
        Assert.Equal(4, unit.HealthModifier);
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