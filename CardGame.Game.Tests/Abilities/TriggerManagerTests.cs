using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game;
using CardGame.Game.Abilities;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Effects;
using CardGame.Game.Game;
using CardGame.Game.Stack;

namespace CardGame.Tests.Trigger;

public sealed class TriggerManagerTests
{
    [Fact]
    public void UnitDestroyed_TriggersMatchingAbility()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var destroyedUnit = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Battlefield
        };

        var triggerSource = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Battlefield
        };

        playerOne.Battlefield.Add(destroyedUnit);
        playerOne.Battlefield.Add(triggerSource);

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;

        var registry = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(registry);

        var abilities = new CardAbilityRegistry();

        abilities.Register(
            CardIds.GraveRat,
            new AbilityDefinition(
                "Test Death Trigger",
                new FriendlyUnitDestroyedTrigger(),
                (_, _, _) => Array.Empty<IEffect>()));

        var context = new GameContext(
            game,
            registry);

        context.CardAbilities.Register(
            CardIds.GraveRat,
            new AbilityDefinition(
                "Test Death Trigger",
                new FriendlyUnitDestroyedTrigger(),
                (_, _, _) => Array.Empty<IEffect>()));

        var engine = new GameEngine(context);

        new DestroyUnitEffect(
            destroyedUnit.InstanceId)
            .Resolve(context);

        Assert.False(context.Stack.IsEmpty);

        var stackItem = context.Stack.Peek();

        Assert.IsType<TriggeredAbilityStackItem>(
            stackItem);

        var triggeredAbility =
            (TriggeredAbilityStackItem)stackItem;

        Assert.Equal(
            triggerSource.InstanceId,
            triggeredAbility.SourceInstanceId);

        Assert.Equal(
            "Test Death Trigger",
            triggeredAbility.AbilityName);
    }

    [Fact]
    public void UnitDestroyed_TriggeredAbility_ResolvesEffect()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var destroyedUnit = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Battlefield
        };

        var triggerSource = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Battlefield
        };

        var cardToDraw = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Deck
        };

        playerOne.Battlefield.Add(destroyedUnit);
        playerOne.Battlefield.Add(triggerSource);
        playerOne.Deck.Add(cardToDraw);

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            playerOne.Id);

        game.Status = GameStatus.InProgress;

        var cardDefinitions = new CardDefinitionRegistry();
        CardCatalog.RegisterAll(cardDefinitions);

        var context = new GameContext(
            game,
            cardDefinitions);

        context.CardAbilities.Register(
            CardIds.GraveRat,
            new AbilityDefinition(
                "Draw on Friendly Death",
                new FriendlyUnitDestroyedTrigger(),
                (ctx, _, sourceId) =>
                [
                    new DrawCardsEffect(
                    ctx.FindCard(sourceId)!.OwnerId,
                    1)
                ]));

        var engine = new GameEngine(context);

        var startingHandSize = playerOne.Hand.Count;

        new DestroyUnitEffect(
            destroyedUnit.InstanceId)
            .Resolve(context);

        Assert.False(context.Stack.IsEmpty);

        Assert.Equal(
            startingHandSize,
            playerOne.Hand.Count);

        Assert.Single(playerOne.Deck);

        engine.StackResolver.TryResolveTop();

        Assert.Equal(
            startingHandSize + 1,
            playerOne.Hand.Count);

        Assert.Empty(playerOne.Deck);

        Assert.Contains(
            cardToDraw,
            playerOne.Hand);

        Assert.Equal(
            ZoneType.Hand,
            cardToDraw.Zone);

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
}