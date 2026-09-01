
using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game;
using CardGame.Game.Cards;
using CardGame.Game.Context;
using CardGame.Game.Game;

namespace CardGame.Tests.Game
{
    public sealed class GameStartTests
    {
        [Fact]
        public void StartGame_ChangesStatusToInProgress_AndStartsFirstTurn()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            context.State.Status = GameStatus.NotStarted;
            context.State.TurnNumber = 1;
            context.State.Phase = GamePhase.Beginning;

            var engine = new GameEngine(context);

            engine.StartGame();

            Assert.Equal(
                GameStatus.InProgress,
                context.State.Status);

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
        public void StartGame_CannotBeCalledTwice()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            context.State.Status = GameStatus.NotStarted;
            context.State.TurnNumber = 1;

            var engine = new GameEngine(context);

            engine.StartGame();

            Assert.Throws<InvalidOperationException>(
                () => engine.StartGame());
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
