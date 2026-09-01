using CardGame.Domain.Cards;
using CardGame.Domain.Game;

namespace CardGame.Game.Cards;

public static class CardCatalog
{
    public static void RegisterAll(CardDefinitionRegistry registry)
    {
        registry.Register(
            new CardDefinition
            {
                Id = CardIds.Morga,
                Name = "Morga, Keeper of the Dead",
                Type = CardType.Leader,
                Color = CardColor.Death,
                EnergyCost = 0,
                BaseHealth = 30
            });

        registry.Register(
            new CardDefinition
            {
                Id = CardIds.GraveRat,
                Name = "Grave Rat",
                Type = CardType.Unit,
                Color = CardColor.Death,
                EnergyCost = 1,
                BaseAttack = 2,
                BaseHealth = 1
            });

        registry.Register(
            new CardDefinition
            {
                Id = CardIds.SacrificialRite,
                Name = "Sacrificial Rite",
                Type = CardType.Spell,
                Color = CardColor.Death,
                EnergyCost = 0
            });
    }
}