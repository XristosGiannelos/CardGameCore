using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game;
using CardGame.Game.Actions;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Game;

namespace CardGame.Tests.Turn
{
    public sealed class GameEngineTurnTests
    {
        [Fact]
        public void StartTurn_StartsActivePlayersTurn()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            context.State.TurnNumber = 2;

            var engine = new GameEngine(context);

            engine.StartTurn();

            Assert.Equal(
                GamePhase.Main,
                context.State.Phase);

            Assert.Equal(
                playerOne.Id,
                context.State.ActivePlayerId);

            Assert.Equal(
                playerOne.Id,
                context.State.PriorityPlayerId);
        }

        [Fact]
        public void EndTurn_SwitchesToNextPlayer()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            context.State.TurnNumber = 2;

            var engine = new GameEngine(context);

            engine.EndTurn();

            Assert.Equal(
                playerTwo.Id,
                context.State.ActivePlayerId);

            Assert.Equal(
                playerTwo.Id,
                context.State.PriorityPlayerId);

            Assert.Equal(
                3,
                context.State.TurnNumber);
        }

        [Fact]
        public void EndTurnAction_EndsCurrentTurn_AndStartsNextPlayersTurn()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            context.State.Status = GameStatus.InProgress;
            context.State.TurnNumber = 2;
            context.State.ActivePlayerId = playerOne.Id;
            context.State.PriorityPlayerId = playerOne.Id;
            context.State.Phase = GamePhase.Main;

            var engine = new GameEngine(context);

            engine.ExecuteAction(
                new EndTurnAction(playerOne.Id));

            Assert.Equal(
                playerTwo.Id,
                context.State.ActivePlayerId);

            Assert.Equal(
                playerTwo.Id,
                context.State.PriorityPlayerId);

            Assert.Equal(
                GamePhase.Main,
                context.State.Phase);

            Assert.Equal(
                3,
                context.State.TurnNumber);
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
