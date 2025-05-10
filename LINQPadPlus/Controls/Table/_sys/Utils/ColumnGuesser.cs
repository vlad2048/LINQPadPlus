using System.Reflection;

namespace LINQPadPlus._sys.Utils;

static class ColumnGuesser
{
	public static ColumnOptions<T>[] Guess<T>() =>
		typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.SelectA(prop =>
				new ColumnOptions<T>(
					GuessFun<T>(prop),
					GuessName<T>(prop),
					prop.PropertyType
				)
				.GuessAlign(prop)
			);


	static Func<T, object?> GuessFun<T>(PropertyInfo prop)
	{
		if (prop.PropertyType == typeof(decimal))
			return item => ((decimal)prop.GetValue(item)!).FmtHuman();
		return item => prop.GetValue(item);
	}

	static string GuessName<T>(PropertyInfo prop) => prop.Name;

	static ColumnOptions<T> GuessAlign<T>(this ColumnOptions<T> opt, PropertyInfo prop)
	{
		if (prop.PropertyType == typeof(decimal))
			opt.Align(ColumnAlign.Right);
		return opt;
	}


	static string FmtHuman(this decimal e) =>
		e switch
		{
			>= 1_000_000_000 => $"{e / 1_000_000_000:n2}B",
			>= 1_000_000 => $"{e / 1_000_000:n2}M",
			>= 1_000 => $"{e / 1_000:n2}K",
			_ => $"{e:n2}",
		};
}