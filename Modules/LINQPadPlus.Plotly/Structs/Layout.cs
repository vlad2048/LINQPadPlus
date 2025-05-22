using System.Text.Json.Serialization;
using LINQPadPlus.Plotly._sys.JsonConverters;

namespace LINQPadPlus.Plotly;

public sealed record Layout
{
	public int? Width { get; init; }
	public int? Height { get; init; }

	[property: JsonConverter(typeof(Write_StringToObjectConverter))]
	public string? Template { get; init; } = Themes.Dark;
	
	public Marg? Margin { get; init; }
	public Legend? Legend { get; init; }
	public Shape[]? Shapes { get; init; }
	public Axis? Xaxis { get; init; }
	public Axis? Yaxis { get; init; }
	public Axis? Yaxis2 { get; init; }
};
