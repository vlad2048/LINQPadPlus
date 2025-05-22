namespace LINQPadPlus.Plotly._sys.Utils;

static class EnumExt
{
	public static U[] SelectA<T, U>(this IEnumerable<T> source, Func<T, U> fun) => source.Select(fun).ToArray();
	public static string JoinLines(this IEnumerable<string> source) => string.Join(Environment.NewLine, source);
}