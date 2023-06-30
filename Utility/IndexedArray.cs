namespace GameCore.Utility;

public record struct IndexedArray<T>(T[] Array, int Index = 0)
{
    public readonly T Current => Array[Index];
    public readonly T this[int i]
    {
        get => Array[i];
        set => Array[i] = value;
    }
}
