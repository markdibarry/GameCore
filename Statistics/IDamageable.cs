namespace GameCore.Statistics;

public interface IDamageable
{
    StatsBase Stats { get; }
    string Name { get; }
}
