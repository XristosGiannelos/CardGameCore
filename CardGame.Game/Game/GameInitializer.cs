using CardGame.Domain.Game;
using CardGame.Domain.Players;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Players;

namespace CardGame.Game.Game;

public sealed class GameInitializer
{
    private readonly DeckManager _deckManager;
    private readonly CardDefinitionRegistry _cardDefinitions;

    public GameInitializer(
        CardDefinitionRegistry cardDefinitions,
        DeckManager? deckManager = null)
    {
        _cardDefinitions = cardDefinitions;
        _deckManager = deckManager ?? new DeckManager();
        CardCatalog.RegisterAll(_cardDefinitions);
    }

    public GameContext Initialize(
        PlayerState playerOne,
        PlayerState playerTwo,
        PlayerState firstPlayer)
    {
        _deckManager.Shuffle(playerOne);
        _deckManager.Shuffle(playerTwo);

        _deckManager.Draw(playerOne, 7);
        _deckManager.Draw(playerTwo, 7);

        var game = GameFactory.Create(
            playerOne,
            playerTwo,
            firstPlayer.Id);

        game.Status = GameStatus.InProgress;

        var context = new GameContext(
                          game,
                          _cardDefinitions);

        CardCatalog.RegisterAll(context.CardDefinitions);

        return context;
    }
}