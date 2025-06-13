namespace LINQPadPlus;

public static class Polyfills
{
#if !NET9_0_OR_GREATER
	public static IEnumerable<(int Index, TSource Item)> Index<TSource>(this IEnumerable<TSource> source) =>
		IsEmptyArray(source) switch
		{
			true => [],
			false => IndexIterator(source),
		};

	static bool IsEmptyArray<TSource>(IEnumerable<TSource> source) => source is TSource[] { Length: 0 };

	static IEnumerable<(int Index, TSource Item)> IndexIterator<TSource>(IEnumerable<TSource> source)
	{
		var index = -1;
		foreach (var element in source)
		{
			checked
			{
				index++;
			}
			yield return (index, element);
		}
	}
#endif
}