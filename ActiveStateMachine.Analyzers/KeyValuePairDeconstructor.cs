namespace ActiveStateMachine.Analyzers;

internal static class KeyValuePairExtensions
{
    internal static void Deconstruct<TKey, TValue>(
        this KeyValuePair<TKey, TValue> pair
        , out TKey key,
        out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }
}