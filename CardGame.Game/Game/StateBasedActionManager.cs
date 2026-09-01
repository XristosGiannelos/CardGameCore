using CardGame.Domain.Cards;
using CardGame.Game.Context;
using CardGame.Game.Effects;

namespace CardGame.Game.Game;

public sealed class StateBasedActionManager
{
    private readonly GameContext _context;
    private readonly GameEndManager _gameEndManager;
    public StateBasedActionManager(GameContext context)
    {
        _context = context;
        _gameEndManager = new GameEndManager(context);
    }

    public void Check()
    {
        var unitsToDestroy = GetDeadUnits();

        foreach (var unit in unitsToDestroy)
        {
            new DestroyUnitEffect(unit.InstanceId)
                .Resolve(_context);
        }

        CheckLeaderDeath();
    }

    private List<CardInstance> GetDeadUnits()
    {
        var deadUnits = new List<CardInstance>();

        foreach (var player in new[]
        {
            _context.State.PlayerOne,
            _context.State.PlayerTwo
        })
        {
            foreach (var unit in player.Battlefield)
            {
                if (_context.GetMaxHealth(unit) <= unit.Damage)
                {
                    deadUnits.Add(unit);
                }
            }
        }

        return deadUnits;
    }
    private void CheckLeaderDeath()
    {
        var playerOne = _context.State.PlayerOne;
        var playerTwo = _context.State.PlayerTwo;

        if (playerOne.Leader.CurrentHealth <= 0)
        {
            _gameEndManager.PlayerLoses(playerOne.Id);
            return;
        }

        if (playerTwo.Leader.CurrentHealth <= 0)
        {
            _gameEndManager.PlayerLoses(playerTwo.Id);
        }
    }
}