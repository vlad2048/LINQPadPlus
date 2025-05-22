using LINQPadPlus.Plotly._sys;

namespace LINQPadPlus.Plotly;

public static class Themes
{
	public static string Dark => dark.Value;
	static readonly Lazy<string> dark = Load(Consts.Resources.ThemeDark);
	


	static Lazy<string> Load(string resourceName) => new(() =>
		ResourceLoader.Load(typeof(Plot).Assembly, resourceName)
			.PlotlyJsonMinify()
	);
}