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
using CardGame.Game.Targets;
using CardGame.Game.Actions;

namespace CardGame.Tests.Abilities;

public sealed class DeathTriggerTests
{
    [Fact]
    public void UnitDeath_TriggersAbility_AndAbilityResolvesThroughStack()
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
            AttackModifier = -1
        };

        var triggerSource = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerTwo.Id,
            ControllerId = playerTwo.Id,
            Zone = ZoneType.Battlefield
        };

        playerTwo.Battlefield.Add(triggerSource);

        playerOne.Battlefield.Add(attacker);
        playerTwo.Battlefield.Add(defender);

        var cardToDraw = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerTwo.Id,
            ControllerId = playerTwo.Id,
            Zone = ZoneType.Deck
        };

        playerTwo.Deck.Add(cardToDraw);

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

        context.CardAbilities.Register(
            CardIds.GraveRat,
            new AbilityDefinition(
                "Draw on Friendly Unit Death",
                new FriendlyUnitDestroyedTrigger(),
                (ctx, _, sourceId) =>
                [
                    new DrawCardsEffect(
                        ctx.FindCard(sourceId)!.ControllerId,
                        1)
                ]));

        var engine = new GameEngine(context);

        var startingHandSize = playerTwo.Hand.Count;

        var attack = new AttackAction(
            playerOne.Id,
            attacker.InstanceId,
            new Target.Unit(defender.InstanceId));

        engine.ExecuteAction(attack);

        //Player Two Passes
        engine.ExecuteAction(
    new PassPriorityAction(playerTwo.Id));

        //Player One Passes
        engine.ExecuteAction(
            new PassPriorityAction(playerOne.Id));

        // Defender died and the death trigger
        // should now be on the stack.
        Assert.Contains(
            defender,
            playerTwo.Graveyard);

        Assert.False(
            context.Stack.IsEmpty);

        Assert.Equal(
            startingHandSize,
            playerTwo.Hand.Count);

        // Resolve the triggered ability resolves.
        //Active player gets rpiority
        engine.ExecuteAction(
            new PassPriorityAction(playerOne.Id));

        engine.ExecuteAction(
            new PassPriorityAction(playerTwo.Id));

        Assert.Equal(
            startingHandSize + 1,
            playerTwo.Hand.Count);

        Assert.Contains(
            cardToDraw,
            playerTwo.Hand);

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