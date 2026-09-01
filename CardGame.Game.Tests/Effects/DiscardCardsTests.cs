using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Effects;
using CardGame.Game.Game;

namespace CardGame.Tests.Effects;

public sealed class DiscardCardsEffectTests
{
    [Fact]
    public void Resolve_MovesCardFromHandToGraveyard()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var card = new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = CardIds.GraveRat,
            OwnerId = playerOne.Id,
            ControllerId = playerOne.Id,
            Zone = ZoneType.Hand
        };

        playerOne.Hand.Add(card);

        var context = CreateContext(
            playerOne,
            playerTwo);

        var effect = new DiscardCardsEffect(
            playerOne.Id,
            1);

        effect.Resolve(context);

        Assert.Empty(playerOne.Hand);
        Assert.Contains(card, playerOne.Graveyard);
        Assert.Equal(
            ZoneType.Graveyard,
            card.Zone);
    }

    [Fact]
    public void Resolve_DiscardsRequestedAmount()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        for (var i = 0; i < 3; i++)
        {
            var card = new CardInstance
            {
                InstanceId = CardInstanceId.New(),
                DefinitionId = CardIds.GraveRat,
                OwnerId = playerOne.Id,
                ControllerId = playerOne.Id,
                Zone = ZoneType.Hand
            };

            playerOne.Hand.Add(card);
        }

        var context = CreateContext(
            playerOne,
            playerTwo);

        var effect = new DiscardCardsEffect(
            playerOne.Id,
            2);

        effect.Resolve(context);

        Assert.Single(playerOne.Hand);
        Assert.Equal(2, playerOne.Graveyard.Count);
    }

    [Fact]
    public void Resolve_WithoutEnoughCards_Throws()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var context = CreateContext(
            playerOne,
            playerTwo);

        var effect = new DiscardCardsEffect(
            playerOne.Id,
            1);

        Assert.Throws<InvalidOperationException>(
            () => effect.Resolve(context));
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