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

public sealed class CastSpellActionTests
{
    [Fact]
    public void CastSpell_FromHand_PaysEnergyAndPutsSpellOnStack()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spell = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.SacrificialRite,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Hand
        };

        playerOne.Hand.Add(spell);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var engine = new GameEngine(context);

        var startingEnergy = playerOne.Energy;

        var action = new PlayCardAction(
            playerOne.Id,
            spell.InstanceId);

        Assert.True(
            action.CanExecute(context));

        engine.ExecuteAction(action);

        Assert.DoesNotContain(
            spell,
            playerOne.Hand);

        Assert.Equal(
            startingEnergy,
            playerOne.Energy);

        Assert.False(
            context.Stack.IsEmpty);
    }

    [Fact]
    public void CastSpell_WhenResolved_MovesSpellToGraveyard()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var spell = new CardInstance
            {
                InstanceId = CardInstanceId.New(),
                DefinitionId = CardIds.SacrificialRite,
                OwnerId = playerOne.Id,
                ControllerId = playerOne.Id,
                Zone = ZoneType.Hand
            };

            playerOne.Hand.Add(spell);

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            var engine = new GameEngine(context);

            var action = new PlayCardAction(
                playerOne.Id,
                spell.InstanceId);

            engine.ExecuteAction(action);

        Assert.DoesNotContain(spell, playerOne.Hand);

        // The spell has been cast, but has not resolved yet.
        Assert.True(
                context.Stack.Count > 0);

        engine.ExecuteAction(
                new PassPriorityAction(playerOne.Id));

        engine.ExecuteAction(
                new PassPriorityAction(playerTwo.Id));



            Assert.DoesNotContain(
                spell,
                playerOne.Hand);

            Assert.Contains(
                spell,
                playerOne.Graveyard);

            Assert.Equal(
                ZoneType.Graveyard,
                spell.Zone);

            Assert.True(
                context.Stack.IsEmpty);
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

