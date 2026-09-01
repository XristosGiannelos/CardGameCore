using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game;
using CardGame.Game.Actions;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGame.Tests.Actions
{
    public sealed class GameActionExecutorTests
    {
        [Fact]
        public void CannotActAfterGameHasFinished()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            context.State.Status = GameStatus.Finished;
            context.State.LoserId = playerOne.Id;
            context.State.WinnerId = playerTwo.Id;

            var engine = new GameEngine(context);

            var action = new PassPriorityAction(playerOne.Id);

            var exception = Assert.Throws<InvalidOperationException>(
                () => engine.ExecuteAction(action));

            Assert.Equal(
                "The game is not in progress.",
                exception.Message);
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
