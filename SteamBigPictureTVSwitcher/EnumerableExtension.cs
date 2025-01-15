namespace SteamBigPictureTVSwitcher;

public static class EnumerableExtension
{
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action, nameof(action));

        foreach (T item in enumerable)
        {
            action(item);
        }
    }
}