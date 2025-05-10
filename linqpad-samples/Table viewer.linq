<Query Kind="Program">
  <NuGetReference>LINQPadPlus</NuGetReference>
  <Namespace>LINQPadPlus</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

void OnStart() => LINQPadPlusSetup.Init();


void Main()
{
	var symbols = Load<Symbol>("data-symbols.json");
	
	symbols.ToTable(Tables.Symbols).Dump();
}


sealed record Symbol(
	string Ticker,
	string Exchange,
	string Mic,
	string Name,
	string Currency
);


static class Tables
{
	/*
	Specify the fields to display using Add()
	And the fields to search on using Search()	
	*/
	public static TableOptions<Symbol> Symbols = new TableOptions<Symbol>(700, 400)
		.Add(e => e.Ticker)
		.Add(e => e.Exchange)
		.Add(e => e.Mic)
		.Add(e => e.Name)
		.Search(e => e.Ticker)
		.Search(e => e.Name);
}


static readonly string samplesFolder = Path.Combine(Path.GetDirectoryName(typeof(LINQPadPlusSetup).Assembly.Location)!, "..", "..", "linqpad-samples");

static T[] Load<T>(string name) => JsonSerializer.Deserialize<T[]>(File.ReadAllText(Path.Combine(samplesFolder, name)))!;
