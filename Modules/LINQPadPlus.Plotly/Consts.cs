namespace LINQPadPlus.Plotly;

static class Consts
{
	// Embedding plotly takes 4 sec on startup vs 0.5 sec for the CDN
	public const string PlotlyUrl = "https://cdn.plot.ly/plotly-3.0.1.min.js";

	public static class Resources
	{
		public const string ThemeDark = $"{ThemeFolder}.dark.json";
		
		const string ThemeFolder = "LINQPadPlus.Plotly._sys.Themes";
	}
}