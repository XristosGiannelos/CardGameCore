using CardGame.Domain.Game;
using CardGame.Game.Context;
using CardGame.Game.Players;

namespace CardGame.Game.Turn;

public sealed class TurnManager
{
    private readonly GameContext _context;
    private readonly DeckManager _deckManager;

    public TurnManager(
        GameContext context,
        DeckManager? deckManager = null)
    {
        _context = context;
        _deckManager = deckManager ?? new DeckManager();
    }

    public void StartTurn()
    {
        var game = _context.State;
        var player = _context.GetPlayer(game.ActivePlayerId);

        game.PriorityPlayerId = game.ActivePlayerId;
        game.ConsecutivePasses = 0;

        game.Phase = GamePhase.Beginning;

        BeginPhase(player);

        game.Phase = GamePhase.Draw;

        DrawPhase(player);

        game.Phase = GamePhase.Main;

        MainPhase(player);
    }

    public void EndTurn()
    {
        var game = _context.State;
        var player = _context.GetPlayer(game.ActivePlayerId);

        game.Phase = GamePhase.End;

        EndPhase(player);

        game.TurnNumber++;

        game.ActivePlayerId =
            game.OpponentOf(player.Id).Id;

        game.PriorityPlayerId =
            game.ActivePlayerId;

        game.ConsecutivePasses = 0;
    }

    private void BeginPhase(
        Domain.Players.PlayerState player)
    {
        player.Energy = player.MaxEnergy;

        player.SpellsCastThisTurn = 0;

        UntapAll(player);

        // Beginnign of turn triggers handled here later?
    }

    private void DrawPhase(
        Domain.Players.PlayerState player)
    {
        var game = _context.State;

        if (game.IsFirstTurn &&
            game.ActivePlayerId == game.FirstPlayerId)
        {
            return;
        }

        if (player.Deck.Count == 0)
        {
            _context.PlayerLoses(player.Id);
            return;
        }

        _deckManager.Draw(player, 1);
    }

    private void MainPhase(
        Domain.Players.PlayerState player)
    {
        // The player remains in Main phase.
        // Actions will be handled by the action system.
    }

    private void EndPhase(
        Domain.Players.PlayerState player)
    {
        foreach (var card in _context.State.PlayerOne.Battlefield)
        {
            card.Damage = 0;
        }

        foreach (var card in _context.State.PlayerTwo.Battlefield)
        {
            card.Damage = 0;
        }

        // Eot triggers handled here later.
    }

    private static void UntapAll(
        Domain.Players.PlayerState player)
    {
        foreach (var card in player.Battlefield)
        {
            card.ReadyState = CardReadyState.Untapped;
        }
    }
}