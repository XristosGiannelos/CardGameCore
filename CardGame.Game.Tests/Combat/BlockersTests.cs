using CardGame.Domain.Cards;
using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Domain.Players;
using CardGame.Game;
using CardGame.Game.Actions;
using CardGame.Game.Cards;
using CardGame.Game.Combat;
using CardGame.Game.Context;
using CardGame.Game.Game;
using CardGame.Game.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGame.Tests.Combat
{
    public sealed class BlockersTests
    {
        [Fact]
        public void UntappedUnit_CanBlock_AndBecomesTapped()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var attacker = CreateUnit(
                playerOne.Id,
                playerOne.Id);

            var blocker = CreateUnit(
                playerTwo.Id,
                playerTwo.Id);

            playerOne.Battlefield.Add(attacker);
            playerTwo.Battlefield.Add(blocker);

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            context.Combat = new CombatState(
                attacker.InstanceId,
                new Target.Leader(playerTwo.Id));

            var action = new DeclareBlockerAction(
                playerTwo.Id,
                attacker.InstanceId,
                blocker.InstanceId);

            Assert.True(
                action.CanExecute(context));

            action.Execute(context);

            Assert.Equal(
                CardReadyState.Tapped,
                blocker.ReadyState);

            Assert.Equal(
                blocker.InstanceId,
                context.Combat.BlockerId);
        }
        [Fact]
        public void TappedUnit_CannotBlock()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var attacker = CreateUnit(
                playerOne.Id,
                playerOne.Id);

            var blocker = CreateUnit(
                playerTwo.Id,
                playerTwo.Id);

            blocker.ReadyState = CardReadyState.Tapped;

            playerOne.Battlefield.Add(attacker);
            playerTwo.Battlefield.Add(blocker);

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            context.Combat = new CombatState(
                attacker.InstanceId,
                new Target.Leader(playerTwo.Id));

            var action = new DeclareBlockerAction(
                playerTwo.Id,
                attacker.InstanceId,
                blocker.InstanceId);

            Assert.False(
                action.CanExecute(context));
        }
        [Fact]
        public void BlockedLeaderAttack_DoesNotDamageLeader()
        {
            var playerOne = CreatePlayer();
            var playerTwo = CreatePlayer();

            var attacker = CreateUnit(
                playerOne.Id,
                playerOne.Id);

            var blocker = CreateUnit(
                playerTwo.Id,
                playerTwo.Id);

            playerOne.Battlefield.Add(attacker);
            playerTwo.Battlefield.Add(blocker);

            var context = CreateContext(
                playerOne,
                playerTwo,
                playerOne.Id);

            var engine = new GameEngine(context);

            var startingHealth =
                playerTwo.Leader.CurrentHealth;

            var attack = new AttackAction(
                playerOne.Id,
                attacker.InstanceId,
                new Target.Leader(playerTwo.Id));

            engine.ExecuteAction(attack);

            var block = new DeclareBlockerAction(
                playerTwo.Id,
                attacker.InstanceId,
                blocker.InstanceId);

            engine.ExecuteAction(block);

            Assert.Equal(
                CardReadyState.Tapped,
                blocker.ReadyState);

            Assert.Equal(
                blocker.InstanceId,
                context.Combat!.BlockerId);

            // Plyer Two passes.
            engine.ExecuteAction(
                new PassPriorityAction(playerTwo.Id));

            engine.ExecuteAction(
                new PassPriorityAction(playerOne.Id));

            Assert.Equal(
                startingHealth,
                playerTwo.Leader.CurrentHealth);

            // Attacker either survives with 2 damage or died from the block.
            if (playerOne.Battlefield.Contains(attacker))
            {
                Assert.Equal(2, attacker.Damage);
            }
            else
            {
                Assert.Contains(attacker, playerOne.Graveyard);
                Assert.Equal(ZoneType.Graveyard, attacker.Zone);
            }
            // Blocker either survives with 2 damage or died from combat.
            if (playerTwo.Battlefield.Contains(blocker))
            {
                Assert.Equal(2, blocker.Damage);
            }
            else { Assert.Contains(blocker, playerTwo.Graveyard); Assert.Equal(ZoneType.Graveyard, blocker.Zone); }

            Assert.True(
                context.Stack.IsEmpty);

            Assert.Null(
                context.Combat);
        }
        private static CardInstance CreateUnit(
    PlayerId ownerId,
    PlayerId controllerId)
        {
            return new CardInstance
            {
                InstanceId = CardInstanceId.New(),
                DefinitionId = CardIds.GraveRat,
                OwnerId = ownerId,
                ControllerId = controllerId,
                Zone = ZoneType.Battlefield
            };
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
}
