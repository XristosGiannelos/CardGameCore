using CardGame.Game.Context;

namespace CardGame.Game.Effects;

public interface IEffect
{
    void Resolve(GameContext context);
}