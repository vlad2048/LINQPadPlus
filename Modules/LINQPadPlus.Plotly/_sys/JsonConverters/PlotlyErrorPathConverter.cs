using System.Text.Json;
using System.Text.Json.Serialization;
using LINQPadPlus.Plotly._sys.Utils;

namespace LINQPadPlus.Plotly._sys.JsonConverters;

sealed class PlotlyErrorPathConverter : JsonConverter<PlotlyErrorPath>
{
	public override PlotlyErrorPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions jsonOpt) =>
		reader.TokenType switch
		{
			JsonTokenType.String => new PlotlyErrorPath(null),
			JsonTokenType.StartArray => new PlotlyErrorPath(ReadStringArray(ref reader)),
			_ => throw new ArgumentException("Unexpected"),
		};

	public override void Write(Utf8JsonWriter writer, PlotlyErrorPath value, JsonSerializerOptions jsonOpt) => throw new NotImplementedException("Not needed");


	static string[] ReadStringArray(ref Utf8JsonReader reader)
	{
		var values = new List<string>();

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndArray)
			{
				return [..values];
			}

			if (reader.TokenType != JsonTokenType.String)
			{
				throw new JsonException("Expected String token");
			}

			values.Add(reader.GetString() ?? throw new NullReferenceException("Ouch"));
		}

		throw new JsonException("Expected EndArray token");
	}
}