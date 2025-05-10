<Query Kind="Program">
  <Namespace>LINQPadPlus</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Serialization</Namespace>
</Query>

#load "lib"



void Main()
{
	var syms = JsonUtils.Load<SymbolSrc[]>(@"C:\ProgramData\Feed.Final\symbology\symbols.json")
		.Select(e => e.Conv())
		.Shuffle(null)
		.Take(4000)
		.Save(@"D:\dev\big\LINQPadPlus\linqpad-samples\data-symbols.json");
	var set = syms.Select(e => e.Mic).ToHashSet();
	var mics = JsonUtils.Load<MicSrc[]>(@"C:\ProgramData\Feed.Final\symbology\download-mics.json")
		.Select(e => e.Conv())
		.Where(e => set.Contains(e.Name))
		.ToArray()
		.Save(@"D:\dev\big\LINQPadPlus\linqpad-samples\data-exchanges.json");
	
	syms.ToTable(Tables.Symbols).p();
	mics.ToTable(Tables.Mics).p();
}


public sealed record SymbolSrc(
	string Figi,
	string Isin,
	string Mic,
	string Cfi,
	string Exchange,
	string Exchange_Trading212,
	int ScheduleId_Trading212,

	string Ticker,
	string Ccy,
	string Country,

	string Name_Trading212,
	string Name_TwelveData
)
{
	public Symbol Conv() => new(
		Ticker,
		Exchange,
		Mic,
		Name_TwelveData,
		Ccy
	);
}




public enum MicStatus
{
	Active,
	Expired,
	Updated,
}

public sealed record MicSrc(
	string Name,
	string OperatingMic,
	bool IsRoot,
	string MarketNameInstitutionDescr,
	string LegalEntityName,
	string Lei,
	string MarketCategoryCode,
	string Acronym,
	string IsoCountry,
	string City,
	string Website,
	MicStatus Status,
	DateOnly CreationDate,
	DateOnly LastUpdateDate,
	DateOnly? LastValidationDate,
	DateOnly? ExpiryDate,
	string Comments,
	int Utils_Level,
	string Utils_FullName
)
{
	public Mic Conv() => new Mic(Name, MarketNameInstitutionDescr, City, Website);
}


public sealed record Symbol(
	string Ticker,
	string Exchange,
	string Mic,
	string Name,
	string Currency
);

public sealed record Mic(
	string Name,
	string InstitutionName,
	string City,
	string Website
);



static class Tables
{
	public static TableOptions<Symbol> Symbols = new TableOptions<Symbol>(null, 200)
		.Add(e => e.Ticker)
		.Add(e => e.Exchange)
		.Add(e => e.Mic)
		.Add(e => e.Name)
		;
	public static TableOptions<Mic> Mics = new TableOptions<Mic>(null, 200)
		.Add(e => e.Name)
		.Add(e => e.InstitutionName)
		.Add(e => e.City)
		;
}





public static class JsonUtils
{
	static readonly JsonSerializerOptions jsonOpt = new()
	{
		WriteIndented = true,
		IndentCharacter = '\t',
		IndentSize = 1,
		Converters = {
			new JsonStringEnumConverter(),
		},
	};

	public static string Ser<T>(this T obj) => JsonSerializer.Serialize(obj, jsonOpt);
	public static T Deser<T>(string str) => JsonSerializer.Deserialize<T>(str, jsonOpt) ?? throw new ArgumentException("Deserialization returned null");
	public static T Save<T>(this T obj, string file)
	{
		File.WriteAllText(file, obj.Ser());
		return obj;
	}
	public static T Load<T>(string file) => Deser<T>(File.ReadAllText(file));
}

public static class EnumExt
{
	public static T[] Shuffle<T>(this IEnumerable<T> source, int? seed)
	{
		var rnd = seed switch
		{
			not null => new Random(seed.Value),
			null => new Random((int)DateTime.Now.Ticks)
		};
		var array = source.ToArray();
		var n = array.Length;
		for (var i = 0; i < n - 1; i++)
		{
			var r = i + rnd.Next(n - i);
			(array[r], array[i]) = (array[i], array[r]);
		}
		return array;
	}
}
