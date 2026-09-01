using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game.Cards;
using CardGame.Game.Combat;
using CardGame.Game.Events;
using CardGame.Game.Stack;
using CardGame.Game.Targets;

namespace CardGame.Game.Context;

public sealed class GameContext
{
    public GameState State { get; }
    public GameStack Stack { get; } = new();
    public GameEventDispatcher Events { get; } = new();
    public CardAbilityRegistry CardAbilities { get; } = new();
    public CardEffectRegistry CardEffects { get; } = new();
    public CardDefinitionRegistry CardDefinitions { get; } = new();
    public CombatState? Combat { get; set; }
    public GameContext(GameState state, CardDefinitionRegistry cardDefinitions)
    {
        State = state;
        CardDefinitions = cardDefinitions;
    }

    public PlayerState GetPlayer(PlayerId playerId)
    {
        return State.GetPlayer(playerId);
    }

    public PlayerState GetOpponent(PlayerId playerId)
    {
        return State.OpponentOf(playerId);
    }

    public CardInstance? FindCard(CardInstanceId instanceId)
    {
        foreach (var player in new[] { State.PlayerOne, State.PlayerTwo })
        {
            var card = FindCardInPlayer(player, instanceId);

            if (card is not null)
                return card;
        }

        return null;
    }

    private static CardInstance? FindCardInPlayer(
        PlayerState player,
        CardInstanceId instanceId)
    {
        return player.Deck.FirstOrDefault(x => x.InstanceId == instanceId)
            ?? player.Hand.FirstOrDefault(x => x.InstanceId == instanceId)
            ?? player.Battlefield.FirstOrDefault(x => x.InstanceId == instanceId)
            ?? player.Graveyard.FirstOrDefault(x => x.InstanceId == instanceId)
            ?? player.Banish.FirstOrDefault(x => x.InstanceId == instanceId);
    }
    public int GetAttack(CardInstance card)
    {
        var definition = CardDefinitions.Get(card.DefinitionId);

        return (definition.BaseAttack ?? 0)
            + card.AttackModifier;
    }
    public int GetMaxHealth(CardInstance card)
    {
        var definition = CardDefinitions.Get(card.DefinitionId);

        return (definition.BaseHealth ?? 0)
            + card.HealthModifier;
    }

    public void PlayerLoses(PlayerId playerId)
    {
        State.Status = GameStatus.Finished;
        State.LoserId = playerId;
        State.WinnerId = State.OpponentOf(playerId).Id;
    }

    public CardInstance? GetTargetUnit(Target.Unit target)
    {
        var card = FindCard(target.InstanceId);

        if (card is null)
            return null;

        if (card.Zone != ZoneType.Battlefield)
            return null;

        var definition = CardDefinitions.Get(card.DefinitionId);

        if (definition.Type != CardType.Unit)
            return null;

        return card;
    }

    public LeaderState? GetTargetLeader(Target.Leader target)
    {
        return GetPlayer(target.PlayerId).Leader;
    }
}