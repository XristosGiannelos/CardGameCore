using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game.Context;
using CardGame.Game.Events;
using CardGame.Game.Stack;
using CardGame.Game.Targets;

namespace CardGame.Game.Actions;

public sealed class PlayCardAction : IGameAction
{
    public PlayerId PlayerId { get; }

    public CardInstanceId CardInstanceId { get; }
    public Target? Target { get; }

    // Actual sped is determined by the card type
    // inside GameActionExecutor.
    public ActionSpeed Speed => ActionSpeed.Main;

    public PlayCardAction(
        PlayerId playerId,
        CardInstanceId cardInstanceId,
        Target? target = null)
    {
        PlayerId = playerId;
        CardInstanceId = cardInstanceId;
        Target = target;
    }

    public bool CanExecute(GameContext context)
    {
        var card = context.FindCard(CardInstanceId);

        if (card is null)
            return false;

        if (card.OwnerId != PlayerId)
            return false;

        if (card.Zone != ZoneType.Hand)
            return false;

        var definition =
            context.CardDefinitions.Get(card.DefinitionId);

        if (definition.Type != CardType.Unit &&
            definition.Type != CardType.Spell)
            return false;

        if (definition.Type == CardType.Spell &&
            Target is not null)
        {
            var targetValidator = new TargetValidator(context);

            if (!targetValidator.IsValid(Target))
                return false;
        }

        var player = context.GetPlayer(PlayerId);

        if (player.Energy < definition.EnergyCost)
            return false;

        if (!IsValidTarget(context))
            return false;

        return true;

    }

    public void Execute(GameContext context)
    {
        if (!CanExecute(context))
        {
            throw new InvalidOperationException(
                "The card cannot be played.");
        }

        var player = context.GetPlayer(PlayerId);

        var card = context.FindCard(CardInstanceId)!;

        var definition =
            context.CardDefinitions.Get(card.DefinitionId);

        player.Energy -= definition.EnergyCost;

        player.Hand.Remove(card);

        switch (definition.Type)
        {
            case CardType.Unit:
                PlayUnit(player, card);
                break;

            case CardType.Spell:
                CastSpell(context, player, card, Target);
                break;

            default:
                throw new InvalidOperationException(
                    $"Card type {definition.Type} cannot be played.");
        }
    }

    private static void PlayUnit(
        PlayerState player,
        CardInstance card)
    {
        card.Zone = ZoneType.Battlefield;

        player.Battlefield.Add(card);
    }

    private static void CastSpell(
        GameContext context,
        PlayerState player,
        CardInstance card,
        Target? target)
    {
        player.SpellsCastThisTurn++;

        var effects =
            context.CardEffects.CreateEffects(
                card.DefinitionId,
                context,
                card.InstanceId,
                target);

        var stackItem = new SpellStackItem(
            player.Id,
            card,
            target,
            effects);

        context.Stack.Push(stackItem);

        context.Events.Dispatch(
            new SpellCastEvent(
                card.InstanceId,
                player.Id,
                false));
    }
    private bool IsValidTarget(GameContext context)
    {
        if (Target is null)
            return true;

        return Target switch
        {
            Target.Unit unitTarget =>
                context.GetTargetUnit(unitTarget) is not null,

            Target.Leader leaderTarget =>
                IsValidLeaderTarget(context, leaderTarget),

            _ => false
        };
    }

    private static bool IsValidLeaderTarget(
        GameContext context,
        Target.Leader target)
    {
        try
        {
            context.GetPlayer(target.PlayerId);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}