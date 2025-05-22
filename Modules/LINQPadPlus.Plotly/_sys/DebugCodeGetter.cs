using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LINQPadPlus.Plotly._sys;

static class DebugCodeGetter
{
	public static string Get(string eltId)
	{
		var allDataStr = JS.Return(
			"""
			(function() {
				const elt = document.getElementById(____0____);
				const { data, layout } = elt;
				return {
					data: data,
					layout: layout
				};
			})();
			""",
			e => e
				.JSRepl_Val(0, eltId)
		);

		var root = Deser<JsonObject>(allDataStr);
		var objData = (JsonArray)root["data"]!;
		var objLayout = (JsonObject)root["layout"]!;
		return
			"""
				import * as Plotly from 'plotly.js-dist-min';

				const data: Plotly.Data[] = ____0____;

				const layout: Partial<Plotly.Layout> = ____1____;

				// const errs = Plotly.validate(data, layout);
				// console.log(errs);

				const plot = await Plotly.newPlot(
					'app',
					data,
					layout
				);

				"""
				.JSRepl_Obj(0, objData.Ser())
				.JSRepl_Obj(1, objLayout.Ser());
	}




	static readonly JsonSerializerOptions jsonOpt = new()
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	};


	static string Ser<T>(this T obj) => JsonSerializer.Serialize(obj, jsonOpt);
	static T Deser<T>(this string str) => JsonSerializer.Deserialize<T>(str, jsonOpt)!;
}