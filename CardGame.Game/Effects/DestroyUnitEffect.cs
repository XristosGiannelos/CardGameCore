using CardGame.Domain.Game;
using CardGame.Domain.Identifiers;
using CardGame.Game.Context;
using CardGame.Game.Events;

namespace CardGame.Game.Effects;

public sealed class DestroyUnitEffect : IEffect
{
    private readonly CardInstanceId _instanceId;

    public DestroyUnitEffect(CardInstanceId instanceId)
    {
        _instanceId = instanceId;
    }

    public void Resolve(GameContext context)
    {
        var unit = context.FindCard(_instanceId);

        if (unit is null)
            return;

        var owner = context.GetPlayer(unit.OwnerId);

        if (unit.Zone != ZoneType.Battlefield)
            return;

        owner.Battlefield.Remove(unit);

        unit.Zone = ZoneType.Graveyard;

        owner.Graveyard.Add(unit);

        unit.Damage = 0;

        context.Events.Dispatch(
            new UnitDestroyedEvent(
            unit.InstanceId,
            unit.OwnerId));
    }   
}