<Query Kind="Program">
  <NuGetReference>LINQPadPlus</NuGetReference>
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>LINQPadPlus</Namespace>
  <Namespace>LINQPadPlus.Rx</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

void OnStart() => LINQPadPlusSetup.Init();


void Main()
{
	var exchanges = Load<Exchange>("data-exchanges.json");
	var symbols = Load<Symbol>("data-symbols.json");

	var (Δexchange, exchangeUI) = exchanges.ToTableSelector(Tables.Exchanges);
	var ΔsymbolsInExchange = Δexchange.SelectVar(exchange => symbols.Where(e => e.Mic == exchange.Name).ToArray());
	var (Δsymbol, symbolUI) = ΔsymbolsInExchange.ToTableSelector(Tables.Symbols);

	tag.Div.style("display:flex; column-gap:20px")[[
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



static T[] Load<T>(string name) => JsonSerializer.Deserialize<T[]>(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath)!, name)))!;
