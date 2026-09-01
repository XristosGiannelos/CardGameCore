using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game.Actions;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Game;
using CardGame.Game.Stack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGame.Tests.Actions
{
    public sealed class EndTurnActionTests
    {
        [Fact]
        public void ActivePlayer_CanEndTurn_WhenMainPhaseAndStackEmpty()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            context.State.Status = GameStatus.InProgress;
            context.State.Phase = GamePhase.Main;
            context.State.ActivePlayerId = playerOne.Id;
            context.State.PriorityPlayerId = playerOne.Id;

            var action = new EndTurnAction(playerOne.Id);

            Assert.True(
                action.CanExecute(context));
        }
        [Fact]
        public void Opponent_CannotEndActivePlayersTurn()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            context.State.Status = GameStatus.InProgress;
            context.State.Phase = GamePhase.Main;
            context.State.ActivePlayerId = playerOne.Id;
            context.State.PriorityPlayerId = playerTwo.Id;

            var action = new EndTurnAction(playerTwo.Id);

            Assert.False(
                action.CanExecute(context));
        }

        [Fact]
        public void CannotEndTurn_WhenStackIsNotEmpty()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            context.State.Status = GameStatus.InProgress;
            context.State.Phase = GamePhase.Main;
            context.State.ActivePlayerId = playerOne.Id;
            context.State.PriorityPlayerId = playerOne.Id;

            var spell = new SpellStackItem(
                playerOne.Id,
                new CardInstance
                {
                    InstanceId = CardInstanceId.New(),
                    DefinitionId = CardIds.SacrificialRite,
                    OwnerId = playerOne.Id,
                    ControllerId = playerOne.Id,
                    Zone = ZoneType.Hand
                },
                null,
                []);

            context.Stack.Push(spell);

            var action = new EndTurnAction(playerOne.Id);

            Assert.False(
                action.CanExecute(context));
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
}
