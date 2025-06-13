using System.Dynamic;
using System.Reflection;

namespace LINQPadPlus.Tabulator._sys.Utils;

static class ColumnGuesser
{
	public static ColumnOptions<T>[] Guess<T>(T item)
	{
		var t = item!.GetType();
		if (t == typeof(ExpandoObject)) return GuessExpando(item);
		return GuessOther<T>(t);
	}

	static ColumnOptions<T>[] GuessExpando<T>(T item)
	{
		if (item is not IDictionary<string, object> map) throw new ArgumentException("Impossible");
		return map.Keys
			.SelectA(e => new ColumnOptions<T>(
				x =>
				{
					if (x is not IDictionary<string, object> xmap) throw new ArgumentException("Impossible");
					return xmap.TryGetValue(e, out var val) switch
					{
						true => val,
						false => "_",
					};
				},
				e,
				typeof(object)
			));
	}


	static ColumnOptions<T>[] GuessOther<T>(Type t) =>
		t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.SelectA(prop =>
				new ColumnOptions<T>(
						GuessFun<T>(prop),
						GuessName(prop),
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

	static string GuessName(PropertyInfo prop) => prop.Name;

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