using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game;
using CardGame.Game.Actions;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Effects;
using CardGame.Game.Game;
using CardGame.Game.Stack;
using CardGame.Game.Targets;

namespace CardGame.Tests.Actions;

public sealed class PlayCardTests
{
    [Fact]
    public void PlayUnit_MovesCardFromHandToBattlefield()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var unit = CreateCard(
            CardIds.GraveRat,
            playerOne.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(unit);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new PlayCardAction(
            playerOne.Id,
            unit.InstanceId);

        Assert.True(action.CanExecute(context));

        action.Execute(context);

        Assert.DoesNotContain(
            unit,
            playerOne.Hand);

        Assert.Contains(
            unit,
            playerOne.Battlefield);

        Assert.Equal(
            ZoneType.Battlefield,
            unit.Zone);
    }

    [Fact]
    public void PlayUnit_PaysEnergy()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var unit = CreateCard(
            CardIds.GraveRat,
            playerOne.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(unit);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var startingEnergy = playerOne.Energy;

        var action = new PlayCardAction(
            playerOne.Id,
            unit.InstanceId);

        action.Execute(context);

        Assert.Equal(
            startingEnergy - 1,
            playerOne.Energy);
    }

    [Fact]
    public void PlayUnit_CannotPlayWithoutEnoughEnergy()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        playerOne.Energy = 0;

        var unit = CreateCard(
            CardIds.GraveRat,
            playerOne.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(unit);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new PlayCardAction(
            playerOne.Id,
            unit.InstanceId);

        Assert.False(
            action.CanExecute(context));
    }

    [Fact]
    public void PlayUnit_CannotPlayCardNotInHand()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var unit = CreateCard(
            CardIds.GraveRat,
            playerOne.Id,
            ZoneType.Battlefield);

        playerOne.Battlefield.Add(unit);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new PlayCardAction(
            playerOne.Id,
            unit.InstanceId);

        Assert.False(
            action.CanExecute(context));
    }

    [Fact]
    public void PlaySpell_MovesCardToStack()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spell = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(spell);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new PlayCardAction(
            playerOne.Id,
            spell.InstanceId);

        Assert.True(
            action.CanExecute(context));

        action.Execute(context);

        Assert.DoesNotContain(
            spell,
            playerOne.Hand);

        Assert.False(
            context.Stack.IsEmpty);

        Assert.Equal(
            spell,
            context.Stack.Peek() is SpellStackItem stackItem
                ? stackItem.Card
                : null);
    }

    [Fact]
    public void PlaySpell_PaysEnergy()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spell = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(spell);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var startingEnergy = playerOne.Energy;

        var action = new PlayCardAction(
            playerOne.Id,
            spell.InstanceId);

        action.Execute(context);

        Assert.Equal(
            startingEnergy,
            playerOne.Energy);
    }

    [Fact]
    public void PlaySpell_IncrementsSpellsCastThisTurn()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spell = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(spell);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var startingCount =
            playerOne.SpellsCastThisTurn;

        var action = new PlayCardAction(
            playerOne.Id,
            spell.InstanceId);

        action.Execute(context);

        Assert.Equal(
            startingCount + 1,
            playerOne.SpellsCastThisTurn);
    }

    [Fact]
    public void CannotPlayOpponentCard()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var unit = CreateCard(
            CardIds.GraveRat,
            playerTwo.Id,
            ZoneType.Hand);

        playerTwo.Hand.Add(unit);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new PlayCardAction(
            playerOne.Id,
            unit.InstanceId);

        Assert.False(
            action.CanExecute(context));
    }

    [Fact]
    public void CannotPlayLeader()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var leader = playerOne.Leader.Card;

        leader.Zone = ZoneType.Hand;

        playerOne.Hand.Add(leader);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new PlayCardAction(
            playerOne.Id,
            leader.InstanceId);

        Assert.False(
            action.CanExecute(context));
    }

    [Fact]
    public void PlaySpell_ResolvesAndMovesToGraveyard()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spell = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(spell);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new PlayCardAction(
            playerOne.Id,
            spell.InstanceId);

        action.Execute(context);

        Assert.False(
            context.Stack.IsEmpty);

        var stackItem =
            Assert.IsType<SpellStackItem>(
                context.Stack.Peek());

        stackItem.Resolve(context);

        //Assert.True(
        //    context.Stack.IsEmpty == false);

        Assert.DoesNotContain(
            spell,
            playerOne.Hand);

        Assert.Contains(
            spell,
            playerOne.Graveyard);

        Assert.Equal(
            ZoneType.Graveyard,
            spell.Zone);
    }
    [Fact]
    public void PlaySpell_CanBePlayedAtInstantSpeed()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spell = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(spell);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var engine = new GameEngine(context);

        // Put the game somewhere other than Main.
        context.State.Phase = GamePhase.Draw;

        var action = new PlayCardAction(
            playerOne.Id,
            spell.InstanceId);

        Assert.True(action.CanExecute(context));

        engine.ExecuteAction(action);

        Assert.False(context.Stack.IsEmpty);
    }

    [Fact]
    public void PlayUnit_CannotBePlayedOutsideMainPhase()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var unit = CreateCard(
            CardIds.GraveRat,
            playerOne.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(unit);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var engine = new GameEngine(context);

        context.State.Phase = GamePhase.Draw;

        var action = new PlayCardAction(
            playerOne.Id,
            unit.InstanceId);

        Assert.True(action.CanExecute(context));

        Assert.Throws<InvalidOperationException>(
            () => engine.ExecuteAction(action));
    }

    [Fact]
    public void CannotPlayCard_WithoutPriority()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var unit = CreateCard(
            CardIds.GraveRat,
            playerTwo.Id,
            ZoneType.Hand);

        playerTwo.Hand.Add(unit);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var engine = new GameEngine(context);

        var action = new PlayCardAction(
            playerTwo.Id,
            unit.InstanceId);

        Assert.True(action.CanExecute(context));

        Assert.Throws<InvalidOperationException>(
            () => engine.ExecuteAction(action));
    }

    [Fact]
    public void PlaySpell_CannotBePlayedWithoutPriority()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spell = CreateCard(
            CardIds.SacrificialRite,
            playerTwo.Id,
            ZoneType.Hand);

        playerTwo.Hand.Add(spell);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var engine = new GameEngine(context);

        context.State.Phase = GamePhase.Draw;

        var action = new PlayCardAction(
            playerTwo.Id,
            spell.InstanceId);

        Assert.True(action.CanExecute(context));

        Assert.Throws<InvalidOperationException>(
            () => engine.ExecuteAction(action));
    }

    [Fact]
    public void Stack_ResolvesTopItemFirst()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spellA = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        var spellB = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(spellA);
        playerOne.Hand.Add(spellB);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var resolutionOrder =
            new List<CardInstanceId>();

        context.CardEffects.Register(
            CardIds.SacrificialRite,
            (_, cardId,_) =>
            [
                new RecordEffect(
                    resolutionOrder,
                    cardId)
            ]);

        var engine = new GameEngine(context);

        // P1 plays Spell A
        engine.ExecuteAction(
            new PlayCardAction(
                playerOne.Id,
                spellA.InstanceId));

        Assert.Equal(
            playerOne.Id,
            context.State.PriorityPlayerId);

        // P1 still has priority and plays Spell B.
        engine.ExecuteAction(
            new PlayCardAction(
                playerOne.Id,
                spellB.InstanceId));

        Assert.Equal(
            spellB.InstanceId,
            ((SpellStackItem)context.Stack.Peek()).Card.InstanceId);

        // P1 passes.
        engine.ExecuteAction(
            new PassPriorityAction(
                playerOne.Id));

        Assert.Equal(
            playerTwo.Id,
            context.State.PriorityPlayerId);

        // P2 passes.
        // Spell B should resolve first because it is on top.
        engine.ExecuteAction(
            new PassPriorityAction(
                playerTwo.Id));

        Assert.Single(
            resolutionOrder);

        Assert.Equal(
            spellB.InstanceId,
            resolutionOrder[0]);

        // Spell A should still be on the stack.
        Assert.False(
            context.Stack.IsEmpty);

        Assert.Equal(
            spellA.InstanceId,
            ((SpellStackItem)context.Stack.Peek()).Card.InstanceId);

        // P1 passes again.
        engine.ExecuteAction(
            new PassPriorityAction(
                playerOne.Id));

        // P2 passes again.
        // Spell A now resolves.
        engine.ExecuteAction(
            new PassPriorityAction(
                playerTwo.Id));

        Assert.Equal(
            2,
            resolutionOrder.Count);

        Assert.Equal(
            spellA.InstanceId,
            resolutionOrder[1]);

        Assert.True(
            context.Stack.IsEmpty);

        Assert.Contains(
            spellA,
            playerOne.Graveyard);

        Assert.Contains(
            spellB,
            playerOne.Graveyard);
    }


    [Fact]
    public void Stack_OpponentCanRespond_AndResponseResolvesFirst()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spellA = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        var spellB = CreateCard(
            CardIds.SacrificialRite,
            playerTwo.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(spellA);
        playerTwo.Hand.Add(spellB);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var resolutionOrder =
            new List<CardInstanceId>();

        context.CardEffects.Register(
            CardIds.SacrificialRite,
            (_, cardId, _) =>
            [
                new RecordEffect(
                resolutionOrder,
                cardId)
            ]);

        var engine = new GameEngine(context);

        // P1 plays A.
        engine.ExecuteAction(
            new PlayCardAction(
                playerOne.Id,
                spellA.InstanceId));

        Assert.Equal(
            playerOne.Id,
            context.State.PriorityPlayerId);

        // P1 passes priority.
        engine.ExecuteAction(
            new PassPriorityAction(
                playerOne.Id));

        // P2 now gets priority.
        Assert.Equal(
            playerTwo.Id,
            context.State.PriorityPlayerId);

        // P2 responds with B.
        engine.ExecuteAction(
            new PlayCardAction(
                playerTwo.Id,
                spellB.InstanceId));

        Assert.Equal(
            playerTwo.Id,
            context.State.PriorityPlayerId);

        Assert.Equal(
            spellB.InstanceId,
            ((SpellStackItem)context.Stack.Peek()).Card.InstanceId);

        // P2 passes.
        engine.ExecuteAction(
            new PassPriorityAction(
                playerTwo.Id));

        // P1 gets priority.
        Assert.Equal(
            playerOne.Id,
            context.State.PriorityPlayerId);

        // P1 passes.
        // Both players passed -> B resolves.
        engine.ExecuteAction(
            new PassPriorityAction(
                playerOne.Id));

        Assert.Single(
            resolutionOrder);

        Assert.Equal(
            spellB.InstanceId,
            resolutionOrder[0]);

        // A is still underneath B.
        Assert.False(
            context.Stack.IsEmpty);

        Assert.Equal(
            spellA.InstanceId,
            ((SpellStackItem)context.Stack.Peek()).Card.InstanceId);

        // P1 passes.
        engine.ExecuteAction(
            new PassPriorityAction(
                playerOne.Id));

        // P2 gets priority.
        Assert.Equal(
            playerTwo.Id,
            context.State.PriorityPlayerId);

        // P2 passes.
        // A resolves.
        engine.ExecuteAction(
            new PassPriorityAction(
                playerTwo.Id));

        Assert.Equal(
            2,
            resolutionOrder.Count);

        Assert.Equal(
            spellA.InstanceId,
            resolutionOrder[1]);

        Assert.True(
            context.Stack.IsEmpty);

    }

    [Fact]
    public void PlaySpell_PreservesTargetOnStack()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spell = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        var target = CreateCard(
            CardIds.GraveRat,
            playerTwo.Id,
            ZoneType.Battlefield);

        playerOne.Hand.Add(spell);
        playerTwo.Battlefield.Add(target);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var engine = new GameEngine(context);

        var action = new PlayCardAction(
            playerOne.Id,
            spell.InstanceId,
            new Target.Unit(target.InstanceId));

        engine.ExecuteAction(action);

        var stackItem =
            Assert.IsType<SpellStackItem>(
                context.Stack.Peek());

        var storedTarget =
            Assert.IsType<Target.Unit>(
                stackItem.Target);

        Assert.Equal(
            target.InstanceId,
            storedTarget.InstanceId);
    }
    private static CardInstance CreateCard(
        CardId definitionId,
        PlayerId ownerId,
        ZoneType zone)
    {
        return new CardInstance
        {
            InstanceId = CardInstanceId.New(),
            DefinitionId = definitionId,
            OwnerId = ownerId,
            ControllerId = ownerId,
            Zone = zone
        };
    }

    [Fact]
    public void CannotExecuteAction_AfterGameHasFinished()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        context.State.Status = GameStatus.Finished;
        context.State.LoserId = playerTwo.Id;
        context.State.WinnerId = playerOne.Id;

        var engine = new GameEngine(context);

        var action = new PassPriorityAction(
            playerOne.Id);

        Assert.Throws<InvalidOperationException>(
            () => engine.ExecuteAction(action));
    }

    [Fact]
    public void PlaySpell_WithUnitTarget_IsValid()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spell = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        var target = CreateCard(
            CardIds.GraveRat,
            playerTwo.Id,
            ZoneType.Battlefield);

        playerOne.Hand.Add(spell);
        playerTwo.Battlefield.Add(target);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new PlayCardAction(
            playerOne.Id,
            spell.InstanceId,
            new Target.Unit(target.InstanceId));

        Assert.True(action.CanExecute(context));
    }

    [Fact]
    public void PlaySpell_WithUnitTarget_NotOnBattlefield_IsInvalid()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spell = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        var target = CreateCard(
            CardIds.GraveRat,
            playerTwo.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(spell);
        playerTwo.Hand.Add(target);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new PlayCardAction(
            playerOne.Id,
            spell.InstanceId,
            new Target.Unit(target.InstanceId));

        Assert.False(action.CanExecute(context));
    }

    [Fact]
    public void PlaySpell_WithLeaderTarget_IsValid()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spell = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(spell);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new PlayCardAction(
            playerOne.Id,
            spell.InstanceId,
            new Target.Leader(playerTwo.Id));

        Assert.True(action.CanExecute(context));
    }

    [Fact]
    public void PlaySpell_WithUnknownLeaderTarget_IsInvalid()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();
        var unknownPlayer = PlayerId.New();

        var spell = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        playerOne.Hand.Add(spell);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var action = new PlayCardAction(
            playerOne.Id,
            spell.InstanceId,
            new Target.Leader(unknownPlayer));

        Assert.False(action.CanExecute(context));
    }

    [Fact]
    public void PlaySpell_WithUnitTarget_DealsDamageWhenResolved()
    {
        var playerOne = CreatePlayer();
        var playerTwo = CreatePlayer();

        var spell = CreateCard(
            CardIds.SacrificialRite,
            playerOne.Id,
            ZoneType.Hand);

        var target = CreateCard(
            CardIds.GraveRat,
            playerTwo.Id,
            ZoneType.Battlefield);

        playerOne.Hand.Add(spell);
        playerTwo.Battlefield.Add(target);

        var context = CreateContext(
            playerOne,
            playerTwo,
            playerOne.Id);

        var effect = new DealDamageEffect(
            new Target.Unit(target.InstanceId),
            2);

        context.CardEffects.Register(
            CardIds.SacrificialRite,
            (_, _, _) => [effect]);

        var action = new PlayCardAction(
            playerOne.Id,
            spell.InstanceId,
            new Target.Unit(target.InstanceId));

        action.Execute(context);

        Assert.Equal(
            0,
            target.Damage);

        var stackItem =
            Assert.IsType<SpellStackItem>(
                context.Stack.Peek());

        stackItem.Resolve(context);

        Assert.Equal(
            2,
            target.Damage);
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



// This exists olny so the test can observe which spell resolves
// first. It does nto belong in the actual game implementation.
public sealed class RecordEffect : IEffect
{
    private readonly List<CardInstanceId> _resolutionOrder;
    private readonly CardInstanceId _cardId;

    public RecordEffect(
        List<CardInstanceId> resolutionOrder,
        CardInstanceId cardId)
    {
        _resolutionOrder = resolutionOrder;
        _cardId = cardId;
    }

    public void Resolve(GameContext context)
    {
        _resolutionOrder.Add(_cardId);
    }
}