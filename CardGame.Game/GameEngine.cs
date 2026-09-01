using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Game.Abilities;
using CardGame.Game.Actions;
using CardGame.Game.Combat;
using CardGame.Game.Context;
using CardGame.Game.Game;
using CardGame.Game.Stack;
using CardGame.Game.Targets;
using CardGame.Game.Turn;

namespace CardGame.Game;

public sealed class GameEngine
{
    public GameContext Context { get; }

    public TriggerManager Triggers { get; }

    public PriorityManager Priority { get; }

    public GameActionExecutor Actions { get; }

    public StackResolver StackResolver { get; }

    public PriorityLoop PriorityLoop { get; }

    public StateBasedActionManager StateBasedActions { get; }

    public TurnManager TurnManager { get; }


    public GameEngine(GameContext context)
    {
        Context = context;

        Triggers = new TriggerManager(context);

        Priority = new PriorityManager(context);

        Actions = new GameActionExecutor(
            context,
            Priority);

        StackResolver = new StackResolver(context);

        var gameEndManager = new GameEndManager(context);

        StateBasedActions = new StateBasedActionManager(context);

        PriorityLoop = new PriorityLoop(
            context,
            Priority,
            StackResolver,
            StateBasedActions);

        TurnManager = new TurnManager(context);

        Context.Events.Subscribe(Triggers);


    }

    public void PassPriority(PlayerId playerId)
    {
        Actions.Execute(
            new PassPriorityAction(playerId));
    }

    public void ExecuteAction(IGameAction action)
    {
        Actions.Execute(action);

        if (Context.State.Status == GameStatus.Finished)
            return;

        if (action is EndTurnAction)
        {
            TurnManager.EndTurn();
            TurnManager.StartTurn();
            return;
        }

        PriorityLoop.ResolveIfBothPlayersPassed();
    }

    public void StartGame()
    {
        if (Context.State.Status != GameStatus.NotStarted)
        {
            throw new InvalidOperationException(
                "The game has already started.");
        }

        Context.State.Status = GameStatus.InProgress;

        TurnManager.StartTurn();
    }

    public void StartTurn()
    {
        TurnManager.StartTurn();
    }

    public void EndTurn()
    {
        TurnManager.EndTurn();
    }
}