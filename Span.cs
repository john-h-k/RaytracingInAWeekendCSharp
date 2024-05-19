public static class SpanExtensions
{
    public static ref T NthElement<T>(this Span<T> @this, int n, Comparison<T> comparison)
    {
        @this.Sort(comparison);

        return ref @this[n];
    }

    public static int Partition<T>(this Span<T> @this, Predicate<T> predicate)
    {
        var first = 0;

        foreach (var e in @this)
        {
            if (!predicate(e))
            {
                break;
            }

            first++;
        }

        if (first == @this.Length - 1)
        {
            return 0;
        }

        for (var i = first + 1; i < @this.Length; i++)
        {
            if (predicate(@this[i]))
            {
                (@this[i], @this[first]) = (@this[first], @this[i]);
                first++;
            }
        }
        
        return first;
    }
}