using System.Text.Json.Nodes;
using LINQPadPlus.Tabulator._sys.Utils;

namespace LINQPadPlus.Tabulator;

static class Consts
{
	public const string TabulatorUrl = "https://unpkg.com/tabulator-tables@6.3.1/dist/js/tabulator.min.js";
	public const string TabulatorStyleUrl = "https://unpkg.com/tabulator-tables@6.3.1/dist/css/tabulator_site_dark.min.css";

	public static readonly JsonObject JsonKeybindings =
		new
			{
				keybindings = new
				{
					moveSelUp = 38,
					moveSelDown = 40,

					moveSelPgUp = 33,
					moveSelPgDown = 34,

					moveSelHome = 36,
					moveSelEnd = 35,
				}
			}
			.ToJsonObjectGen();
}