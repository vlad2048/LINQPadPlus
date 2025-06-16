namespace LINQPadPlus.BuildSystem._sys.Utils;

static class EnumExt
{
	public static U[] SelectA<T, U>(this IEnumerable<T> source, Func<T, U> fun) => [..source.Select(fun)];
	public static T[] WhereA<T>(this IEnumerable<T> source, Func<T, bool> fun) => [.. source.Where(fun)];
}