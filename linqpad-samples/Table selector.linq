<Query Kind="Program">
  <NuGetReference>LINQPadPlus</NuGetReference>
  <NuGetReference>LINQPadPlus.Tabulator</NuGetReference>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>LINQPadPlus</Namespace>
  <Namespace>LINQPadPlus.Rx</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>LINQPadPlus.Tabulator</Namespace>
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
	var exchanges = Load<Exchange>("data-exchanges.json");
	var symbols = Load<Symbol>("data-symbols.json");

	var (Δexchange, exchangeUI) = exchanges.ToTableSelector(Tables.Exchanges);
	var ΔsymbolsInExchange = Var.Expr(() => symbols.Where(e => e.Mic == Δexchange.V.Name).ToArray());
	var (Δsymbol, symbolUI) = ΔsymbolsInExchange.ToTableSelector(Tables.Symbols);

	t.Div.style("display:flex; column-gap:20px")[[
		exchangeUI,
		symbolUI,
		Δsymbol.ToDC(),
	]].Dump();
}


sealed record Symbol(
	string Ticker,
	string Exchange,
	string Mic,
	string Name,
	string Currency
);

sealed record Exchange(
	string Name,
	string InstitutionName,
	string City,
	string Website
);


static class Tables
{
	public static TableOptions<Symbol> Symbols = new TableOptions<Symbol>(700, 400)
		.Add(e => e.Ticker)
		.Add(e => e.Exchange)
		.Add(e => e.Mic)
		.Add(e => e.Name)
		.Search(e => e.Ticker)
		.Search(e => e.Name);
		
	public static TableOptions<Exchange> Exchanges = new TableOptions<Exchange>(500, 400)
		.Add(e => e.Name)
		.Add(e => e.InstitutionName)
		.Add(e => e.City)
		.Search(e => e.Name)
		.Search(e => e.InstitutionName);
}



static readonly string samplesFolder = Path.Combine(Path.GetDirectoryName(typeof(LINQPadPlusSetup).Assembly.Location)!, "..", "..", "linqpad-samples");

static T[] Load<T>(string name) => JsonSerializer.Deserialize<T[]>(File.ReadAllText(Path.Combine(samplesFolder, name)))!;
