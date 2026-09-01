using CardGame.Domain.Cards;

namespace CardGame.Domain.Players;

public sealed class LeaderState
{
    public CardInstance Card { get; }

    public int MaxHealth { get; }

    public int CurrentHealth { get; private set; }

    public Dictionary<string, int> Counters { get; } = [];

    public LeaderState(
        CardInstance card,
        int maxHealth)
    {
        Card = card;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        CurrentHealth = Math.Max(0, CurrentHealth - amount);
    }

    public void Heal(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        CurrentHealth = Math.Min(
            MaxHealth,
            CurrentHealth + amount);
    }

    public int GetCounter(string counterName)
    {
        return Counters.GetValueOrDefault(counterName);
    }

    public void AddCounter(string counterName, int amount = 1)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        Counters[counterName] =
            GetCounter(counterName) + amount;
    }

    public bool RemoveCounter(string counterName, int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        var current = GetCounter(counterName);

        if (current < amount)
            return false;

        Counters[counterName] = current - amount;

        return true;
    }
}