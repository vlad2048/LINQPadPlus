<Query Kind="Program">
  <NuGetReference>LINQPadPlus</NuGetReference>
  <NuGetReference>LINQPadPlus.Tabulator</NuGetReference>
  <Namespace>LINQPadPlus</Namespace>
  <Namespace>LINQPadPlus.Tabulator</Namespace>
  <Namespace>System.Dynamic</Namespace>
</Query>

void OnStart()
{
	LINQPadPlusSetup.Init();
	LINQPadPlusTabulatorSetup.Init();

	Util.HtmlHead.AddCssLink("https://cdn.jsdelivr.net/npm/@picocss/pico@2/css/pico.min.css");
	JS.Run("""document.documentElement.setAttribute("data-theme", "dark");""");
	Util.HtmlHead.AddStyles("""
	:root {
		--pico-spacing: 0rem;								/* 1rem */
		--pico-form-element-spacing-vertical: 0.375rem;		/* 0.75rem */
		--pico-form-element-spacing-horizontal: 0.5rem;		/* 1rem */
	}
	""");
}


void Main()
{
	// Display normal records
	data.ToTable(Tables.Foo).Dump();

	// Display ExpandoObjects
	data.ToDyna().ToTable(Tables.Dyna).Dump();
}





static class Tables
{
	public static TableOptions<Foo> Foo = new TableOptions<Foo>(300, 150)
		.Search(e => e.SuperLongName);

	public static TableOptions<object> Dyna = new TableOptions<object>(300, 150)
		.Search(e => ((IDictionary<string, object>)e)["SuperLongName"]);
}




public record Foo(string SuperLongName, int Num);

public static readonly Foo[] data = [
	new Foo("My super long long name", 123),
	new Foo("OkName", 456),
];






public static class DynaConverter
{
	public static object[] ToDyna(this Foo[] xs) =>
		xs.SelectA(x =>
		{
			var expando = new ExpandoObject();
			IDictionary<string, object> dict = expando!;
			dict["SuperLongName"] = x.SuperLongName;
			dict["Num"] = x.Num;
			return expando;
		});



	public static U[] SelectA<T, U>(this IEnumerable<T> source, Func<T, U> fun) => source.Select(fun).ToArray();
}

