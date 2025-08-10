namespace ActiveStateMachine.Analyzers;

public static class CollectionExtensions
{
    public static HashSet<TSource> ToHashSet<TSource>(
        this IEnumerable<TSource> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return [..source];
    }

    public static HashSet<TSource> ToHashSet<TSource>(
        this IEnumerable<TSource> source,
        IEqualityComparer<TSource> comparer)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return new(source, comparer);
    }
}